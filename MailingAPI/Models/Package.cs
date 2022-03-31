namespace MailingAPI.Models
{
    public class Package
    {
        public int Id { get; set; }
        public string? RecipientAddress { get; set; }
        public string? RecipientName { get; set; }
        public int RecipientZipCode { get; set; }
        public PackageType Type { get; set; }
        public string? TrackingNumber { get; set; }
        public HashSet<TrackingHistory> TrackingHistory { get; set; } = new HashSet<TrackingHistory>();
    }
    public enum PackageType
    {
        Letter,
        Package
    }
}