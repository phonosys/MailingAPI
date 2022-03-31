namespace MailingAPI.Models
{
    public class TrackingHistory
    {
        public int Id { get; set; }
        public TrackingStatus Status { get; set; }
        public DateTime EventTime { get; set; }
    }
    public enum TrackingStatus
    {
        ArrivalToIntermediate,
        DepartureFromIntermediate,
        ArrivalToDestination
    }
}
