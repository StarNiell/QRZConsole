#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QRZLibrary
{
    /// <summary>
    /// Drives an off-screen WebView2: navigation that can be awaited, JavaScript evaluation with
    /// typed (JSON) results, and waits with a timeout that say what they were waiting for.
    /// </summary>
    /// <remarks>
    /// Every value passed to JavaScript goes through <see cref="Js"/>, which produces a proper
    /// JavaScript string literal: quotes or backslashes in a comment can no longer break a script.
    /// </remarks>
    internal sealed class BrowserSession
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly WebView2 _view;
        private TaskCompletionSource<bool>? _pendingNavigation;

        private BrowserSession(WebView2 view)
        {
            _view = view;
            _view.NavigationCompleted += OnNavigationCompleted;
        }

        /// <summary>Raised with a description of timeouts, script errors and (when <see cref="Verbose"/>) every step.</summary>
        public event Action<string>? Diagnostic;

        public bool Verbose { get; set; }

        /// <summary>Default time allowed for a page or an element to appear.</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>Interval between checks while waiting.</summary>
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(50);

        public string Title => _view.CoreWebView2?.DocumentTitle ?? string.Empty;

        public string Url => _view.CoreWebView2?.Source ?? string.Empty;

        public static async Task<BrowserSession> CreateAsync()
        {
            var view = new WebView2();
            string userDataFolder = Path.Combine(Path.GetTempPath(), "QRZConsole");
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, new CoreWebView2EnvironmentOptions());
            await view.EnsureCoreWebView2Async(environment);
            return new BrowserSession(view);
        }

        /// <summary>JavaScript string literal for a .NET string (quotes, backslashes and new lines escaped).</summary>
        public static string Js(string? value) => JsonSerializer.Serialize(value ?? string.Empty);

        #region Navigation

        /// <summary>Loads a page and waits until it has finished loading. False on error page or timeout.</summary>
        public Task<bool> NavigateAsync(string url)
        {
            Trace($"navigate {url}");
            return WaitForNavigationAsync(() =>
            {
                _view.CoreWebView2.Navigate(url);
                return Task.CompletedTask;
            }, $"page {url}");
        }

        /// <summary>Runs an action that makes the page navigate (form submit, link) and waits for the new page.</summary>
        public async Task<bool> WaitForNavigationAsync(Func<Task> trigger, string what)
        {
            var navigation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingNavigation = navigation;

            await trigger();

            Task finished = await Task.WhenAny(navigation.Task, Task.Delay(Timeout));
            if (finished != navigation.Task)
            {
                _pendingNavigation = null;
                Report($"Timeout ({Timeout.TotalSeconds:0}s) waiting for {what}");
                return false;
            }

            bool ok = navigation.Task.Result;
            if (!ok)
                Report($"Navigation failed: {what} ({Url})");
            else
                Trace($"loaded \"{Title}\" {Url}");
            return ok;
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            TaskCompletionSource<bool>? pending = _pendingNavigation;
            _pendingNavigation = null;
            pending?.TrySetResult(e.IsSuccess);
        }

        #endregion

        #region JavaScript

        /// <summary>
        /// Evaluates a JavaScript expression and converts the result from JSON.
        /// Errors in the expression (e.g. a missing element) give default(T).
        /// </summary>
        public async Task<T?> EvalAsync<T>(string expression)
        {
            string script = $"(() => {{ try {{ return ({expression}); }} catch (e) {{ return null; }} }})()";
            string json = await _view.CoreWebView2.ExecuteScriptAsync(script);
            if (string.IsNullOrEmpty(json) || json == "null")
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                Report($"Unexpected script result ({ex.Message}) for: {Shorten(expression)}");
                return default;
            }
        }

        /// <summary>Evaluates a JavaScript boolean expression; false on error.</summary>
        public async Task<bool> TestAsync(string expression) => await EvalAsync<bool>(expression);

        /// <summary>Evaluates a JavaScript expression that returns text; empty string on error.</summary>
        public async Task<string> TextAsync(string expression) => await EvalAsync<string>(expression) ?? string.Empty;

        /// <summary>Runs JavaScript statements. Returns false (and reports the error) if they throw.</summary>
        public async Task<bool> RunAsync(string statements)
        {
            string script = $"(() => {{ try {{ {statements}; return ''; }} catch (e) {{ return String(e); }} }})()";
            string error = JsonSerializer.Deserialize<string>(await _view.CoreWebView2.ExecuteScriptAsync(script)) ?? string.Empty;
            if (error.Length > 0)
            {
                Report($"Script error: {error} in: {Shorten(statements)}");
                return false;
            }

            Trace($"run {Shorten(statements)}");
            return true;
        }

        public Task<bool> ExistsAsync(string cssSelector) => TestAsync($"document.querySelector({Js(cssSelector)}) !== null");

        public Task<bool> IdExistsAsync(string id) => TestAsync($"document.getElementById({Js(id)}) !== null");

        /// <summary>Sets the value property of the element with the given id.</summary>
        public Task<bool> SetValueAsync(string id, string value) => RunAsync($"document.getElementById({Js(id)}).value = {Js(value)}");

        /// <summary>Current value property of an element.</summary>
        public Task<string> GetValueAsync(string id) => TextAsync($"document.getElementById({Js(id)}).value");

        /// <summary>Value attribute as sent by the server (not changed by typing).</summary>
        public Task<string> GetValueAttributeAsync(string id) => TextAsync($"document.getElementById({Js(id)}).getAttribute('value')");

        /// <summary>Sets the value property of the first element matching a CSS selector.</summary>
        public Task<bool> SetValueBySelectorAsync(string cssSelector, string value)
            => RunAsync($"document.querySelector({Js(cssSelector)}).value = {Js(value)}");

        public Task<bool> ClickAsync(string id) => RunAsync($"document.getElementById({Js(id)}).click()");

        #endregion

        #region Waiting

        /// <summary>Polls a condition until it is true or the timeout expires (reported as a diagnostic).</summary>
        public async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, string what, TimeSpan? timeout = null, bool reportTimeout = true)
        {
            TimeSpan limit = timeout ?? Timeout;
            var watch = Stopwatch.StartNew();
            Trace($"wait for {what}");

            while (true)
            {
                if (await condition())
                {
                    Trace($"  ok after {watch.ElapsedMilliseconds} ms: {what}");
                    return true;
                }

                if (watch.Elapsed >= limit)
                {
                    if (reportTimeout)
                        Report($"Timeout ({limit.TotalSeconds:0.#}s) waiting for {what}");
                    return false;
                }

                await Task.Delay(PollInterval);
            }
        }

        public Task<bool> WaitForAsync(string jsCondition, string what, TimeSpan? timeout = null)
            => WaitUntilAsync(() => TestAsync(jsCondition), what, timeout);

        public Task<bool> WaitForElementAsync(string cssSelector, TimeSpan? timeout = null)
            => WaitUntilAsync(() => ExistsAsync(cssSelector), $"element {cssSelector}", timeout);

        /// <summary>
        /// Waits for an AJAX reload signalled by a spinner element: gives the spinner up to
        /// <paramref name="appearWithin"/> to show up, then waits until it is hidden again.
        /// </summary>
        public async Task<bool> WaitForSpinnerAsync(string spinnerId, TimeSpan appearWithin)
        {
            string spinner = $"document.getElementById({Js(spinnerId)})";

            await WaitUntilAsync(() => TestAsync($"{spinner} !== null && !{IsHidden(spinner)}"),
                                 $"spinner #{spinnerId} to appear", appearWithin, reportTimeout: false);

            return await WaitForAsync($"{spinner} !== null && {IsHidden(spinner)}", $"spinner #{spinnerId} to disappear");
        }

        /// <summary>
        /// After a page load: if the page starts an AJAX reload (spinner shown within
        /// <paramref name="appearWithin"/>), waits for it to finish. A missing spinner counts as idle;
        /// a timeout is reported but does not fail the caller (the page itself has loaded).
        /// </summary>
        public async Task WaitForIdleAsync(string spinnerId, TimeSpan appearWithin)
        {
            string spinner = $"document.getElementById({Js(spinnerId)})";
            string idle = $"({spinner} === null || {IsHidden(spinner)})";

            bool appeared = await WaitUntilAsync(() => TestAsync($"!{idle}"), $"spinner #{spinnerId} after load", appearWithin, reportTimeout: false);
            if (appeared)
                await WaitForAsync(idle, $"spinner #{spinnerId} to disappear");
        }

        /// <summary>
        /// JavaScript test "element is hidden": inline style display:none (what the logbook sets after
        /// a reload, the check used by the old code) or hidden by CSS.
        /// </summary>
        private static string IsHidden(string element)
        {
            return $"(s => /display:\\s*none/.test(s.getAttribute('style') || '') || getComputedStyle(s).display === 'none' " +
                   $"|| getComputedStyle(s).visibility === 'hidden')({element})";
        }

        #endregion

        #region Diagnostics

        /// <summary>Always reported: something went wrong.</summary>
        public void Report(string message)
        {
            Debug.WriteLine($"[browser] {message}");
            Diagnostic?.Invoke(message);
        }

        /// <summary>Reported only in verbose mode.</summary>
        public void Trace(string message)
        {
            Debug.WriteLine($"[browser] {message}");
            if (Verbose)
                Diagnostic?.Invoke(message);
        }

        private static string Shorten(string script)
        {
            string oneLine = script.Replace("\r", " ").Replace("\n", " ");
            return oneLine.Length <= 120 ? oneLine : oneLine.Substring(0, 117) + "...";
        }

        #endregion
    }
}
