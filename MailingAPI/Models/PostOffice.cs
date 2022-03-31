namespace MailingAPI.Models
{
    public class PostOffice
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int ZipCode { get; set; }
    }
}