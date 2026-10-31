using System;
using System.Collections.Generic;
using System.Text.Json;

namespace QRZConsole.Core.Settings
{
    /// <summary>
    /// Typed access to the persisted settings. Key names are the ones used by earlier
    /// versions, so existing users keep their data.
    /// </summary>
    public sealed class AppSettings
    {
        private readonly ISettingsStore _store;

        public AppSettings(ISettingsStore store)
        {
            _store = store;
        }

        public string Username
        {
            get => _store.Get("username") ?? string.Empty;
            set => _store.Set("username", value ?? string.Empty);
        }

        public string Password
        {
            get => _store.GetSecret("password") ?? string.Empty;
            set => _store.SetSecret("password", value ?? string.Empty);
        }

        /// <summary>Last callsign looked up / in use.</summary>
        public string LastLookup
        {
            get => _store.Get("lookup") ?? string.Empty;
            set => _store.Set("lookup", value ?? string.Empty);
        }

        public string LastFrequency
        {
            get => _store.Get("lastFreq") ?? string.Empty;
            set => _store.Set("lastFreq", value ?? string.Empty);
        }

        public string ViewMode
        {
            get => _store.Get("currentViewMode") ?? string.Empty;
            set => _store.Set("currentViewMode", value ?? string.Empty);
        }

        /// <summary>Login status as last seen (the site session may have expired since).</summary>
        public bool IsLoggedIn
        {
            get => _store.Get("IsLoggedIn") == "1";
            set => _store.Set("IsLoggedIn", value ? "1" : "0");
        }

        public bool CheckIsLoggedAtStartup
        {
            get => _store.Get("CheckIsLoggedIn") == "1";
            set => _store.Set("CheckIsLoggedIn", value ? "1" : "0");
        }

        public bool ShowHeader
        {
            get => _store.Get("ShowHeader") == "1";
            set => _store.Set("ShowHeader", value ? "1" : "0");
        }

        /// <summary>Grid locators worked (key) and confirmed (value), saved by older versions.</summary>
        public List<KeyValuePair<string, bool>> GetLocators(string username)
        {
            string? json = _store.Get($"{username}_Locators");
            if (string.IsNullOrWhiteSpace(json))
                return new List<KeyValuePair<string, bool>>();

            try
            {
                return JsonSerializer.Deserialize<List<KeyValuePair<string, bool>>>(json)
                       ?? new List<KeyValuePair<string, bool>>();
            }
            catch (JsonException)
            {
                return new List<KeyValuePair<string, bool>>();
            }
        }
    }
}
