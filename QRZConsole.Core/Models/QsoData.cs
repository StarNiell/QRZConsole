namespace QRZConsole.Core.Models
{
    /// <summary>
    /// Data typed by the user to add or edit a QSO.
    /// Frequency is in MHz (e.g. "7.076"); Date is yyyy-MM-dd; Time is HH:mm (UTC).
    /// </summary>
    public sealed record QsoData(string Call, string Frequency, string Mode, string Date, string Time, string Comment);
}
