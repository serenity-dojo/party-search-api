namespace PartySearchApi.Api.Models
{
    public class Party
    {
        public string PartyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PartyType Type { get; set; }
        public SanctionsStatus SanctionsStatus { get; set; } = SanctionsStatus.PendingReview;
        public decimal MatchScore { get; set; } = 0.0m;
    }

    public enum PartyType
    {
        Individual,
        Organization
    }

    public enum SanctionsStatus
    {
        Approved,
        PendingReview,
        Escalated,
        ConfirmedMatch,
        FalsePositive
    }
}