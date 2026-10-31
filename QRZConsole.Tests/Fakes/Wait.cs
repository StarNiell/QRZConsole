namespace QRZConsole.Tests.Fakes
{
    internal static class Wait
    {
        /// <summary>Polls until the condition is true; fails the test after the timeout.</summary>
        public static async Task UntilAsync(Func<bool> condition, string what, int timeoutMs = 5000)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (!condition())
            {
                if (watch.ElapsedMilliseconds > timeoutMs)
                    Assert.Fail($"Timed out waiting for {what}");
                await Task.Delay(20);
            }
        }
    }
}
