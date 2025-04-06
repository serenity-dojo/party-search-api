namespace PartySearchApi.AcceptanceTests.Model
{
    public class PartyDto
    {
        public required string PartyId { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string SanctionsStatus { get; set; }
        public required string MatchScore { get; set; }
    }
}