namespace QRZConsole.Core.Models
{
    /// <summary>Frequency range of an amateur band, in MHz.</summary>
    public sealed record BandFrequencyRange(string Band, double Min, double Max);
}
