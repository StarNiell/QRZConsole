#nullable enable
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using QRZConsole.Core.Settings;

namespace QRZConsole
{
    /// <summary>
    /// Settings in HKCU\SOFTWARE\{product name}, the same key used by earlier versions.
    /// Secrets (the password) are encrypted with Windows DPAPI for the current user;
    /// plain-text values written by older versions are still read, and get encrypted on the next save.
    /// </summary>
    internal sealed class RegistrySettingsStore : ISettingsStore
    {
        private const string ProtectedPrefix = "dpapi:";
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("QRZConsole.settings");

        private readonly string _keyPath;

        public RegistrySettingsStore(string productName)
        {
            _keyPath = $@"SOFTWARE\{productName}";
        }

        public string? Get(string key)
        {
            using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(_keyPath);
            return regKey?.GetValue(key)?.ToString();
        }

        public void Set(string key, string value)
        {
            using RegistryKey regKey = Registry.CurrentUser.CreateSubKey(_keyPath);
            regKey.SetValue(key, value ?? string.Empty);
        }

        public string? GetSecret(string key)
        {
            string? stored = Get(key);
            if (stored == null || !stored.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
                return stored; // missing, or legacy plain text

            try
            {
                byte[] data = Convert.FromBase64String(stored.Substring(ProtectedPrefix.Length));
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(data, Entropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception ex) when (ex is CryptographicException || ex is FormatException)
            {
                return null; // encrypted by another Windows user or corrupted
            }
        }

        public void SetSecret(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Set(key, string.Empty);
                return;
            }

            byte[] data = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Entropy, DataProtectionScope.CurrentUser);
            Set(key, ProtectedPrefix + Convert.ToBase64String(data));
        }
    }
}
