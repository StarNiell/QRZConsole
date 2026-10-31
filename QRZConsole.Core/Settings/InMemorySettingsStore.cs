using System.Collections.Generic;

namespace QRZConsole.Core.Settings
{
    /// <summary>Non-persistent store, used by tests.</summary>
    public sealed class InMemorySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public string? Get(string key) => _values.TryGetValue(key, out string? value) ? value : null;

        public void Set(string key, string value) => _values[key] = value;

        public string? GetSecret(string key) => Get(key);

        public void SetSecret(string key, string value) => Set(key, value);
    }
}
