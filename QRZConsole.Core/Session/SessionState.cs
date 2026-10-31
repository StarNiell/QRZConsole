using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QRZConsole.Core.Session
{
    /// <summary>
    /// In-memory state shared by the commands ("last data in memory").
    /// Raises PropertyChanged so the UI can mirror it (header text boxes).
    /// </summary>
    public sealed class SessionState : INotifyPropertyChanged
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _currentQrz = string.Empty;
        private string _lastFrequency = "7100";
        private string _lastMode = "SSB";
        private ViewMode _viewMode = ViewMode.Raw;
        private int _currentPage = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Username { get => _username; set => SetField(ref _username, value ?? string.Empty); }

        public string Password { get => _password; set => SetField(ref _password, value ?? string.Empty); }

        /// <summary>Callsign currently in use (last lookup, last QSO, set with qq).</summary>
        public string CurrentQrz { get => _currentQrz; set => SetField(ref _currentQrz, value ?? string.Empty); }

        /// <summary>Last frequency, as typed (kHz without dot, e.g. 7100).</summary>
        public string LastFrequency { get => _lastFrequency; set => SetField(ref _lastFrequency, value ?? string.Empty); }

        public string LastMode { get => _lastMode; set => SetField(ref _lastMode, value ?? string.Empty); }

        public ViewMode ViewMode { get => _viewMode; set => SetField(ref _viewMode, value); }

        /// <summary>Logbook page the user is on.</summary>
        public int CurrentPage { get => _currentPage; set => SetField(ref _currentPage, value); }

        /// <summary>Grid locators worked/confirmed (loaded from older saved data, used by lu and gl).</summary>
        public List<KeyValuePair<string, bool>> Locators { get; set; } = new List<KeyValuePair<string, bool>>();

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
