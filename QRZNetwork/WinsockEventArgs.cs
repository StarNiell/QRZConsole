using System;
using System.Net.Sockets;

namespace QRZNetwork
{
    /// <summary>
    /// Connection state, modelled on the VB6 Winsock StateConstants.
    /// </summary>
    public enum WinsockState
    {
        /// <summary>No connection (sckClosed).</summary>
        Closed,
        /// <summary>Connection attempt in progress (sckConnecting).</summary>
        Connecting,
        /// <summary>Connected (sckConnected).</summary>
        Connected,
        /// <summary>Close in progress (sckClosing).</summary>
        Closing,
        /// <summary>The last operation failed (sckError). Connect() can be called again.</summary>
        Error
    }

    /// <summary>Raised when a block of data arrives (VB6: DataArrival).</summary>
    public sealed class WinsockDataArrivalEventArgs : EventArgs
    {
        public WinsockDataArrivalEventArgs(byte[] data, string text)
        {
            Data = data;
            Text = text;
        }

        /// <summary>Raw bytes received in this block.</summary>
        public byte[] Data { get; }

        /// <summary>The same block decoded with <see cref="WinsockClient.Encoding"/>.</summary>
        public string Text { get; }

        /// <summary>Number of bytes received (VB6: bytesTotal).</summary>
        public int BytesTotal => Data.Length;
    }

    /// <summary>Raised for every complete text line received (terminator removed).</summary>
    public sealed class WinsockLineReceivedEventArgs : EventArgs
    {
        public WinsockLineReceivedEventArgs(string line) => Line = line;

        public string Line { get; }
    }

    /// <summary>Raised when a queued send has been written to the socket (VB6: SendComplete).</summary>
    public sealed class WinsockSendCompleteEventArgs : EventArgs
    {
        public WinsockSendCompleteEventArgs(int bytesSent) => BytesSent = bytesSent;

        public int BytesSent { get; }
    }

    /// <summary>Raised when a socket error occurs (VB6: Error).</summary>
    public sealed class WinsockErrorEventArgs : EventArgs
    {
        public WinsockErrorEventArgs(SocketError errorCode, string message, Exception? exception)
        {
            ErrorCode = errorCode;
            Message = message;
            Exception = exception;
        }

        public SocketError ErrorCode { get; }

        public string Message { get; }

        public Exception? Exception { get; }
    }
}
