namespace QRZConsole.Core.Settings
{
    /// <summary>Key/value persistence for user settings (the Windows app uses the registry).</summary>
    public interface ISettingsStore
    {
        /// <summary>Returns the stored value, or null when the key does not exist.</summary>
        string? Get(string key);

        void Set(string key, string value);

        /// <summary>Reads a value stored with <see cref="SetSecret"/> (and legacy plain-text values).</summary>
        string? GetSecret(string key);

        /// <summary>Stores a value protected for the current user (e.g. the QRZ password).</summary>
        void SetSecret(string key, string value);
    }
}
