namespace QRZConsole.Core.Models
{
    /// <summary>Result of a callsign lookup on QRZ.com.</summary>
    public class LookupEntry
    {
        public string QRZ { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string DXCC { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Address3 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GridSquare { get; set; } = string.Empty;
        public string Lookups { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string UsState { get; set; } = string.Empty;
        public string UsCounty { get; set; } = string.Empty;
    }
}
