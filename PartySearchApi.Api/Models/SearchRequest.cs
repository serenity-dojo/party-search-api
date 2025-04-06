namespace PartySearchApi.Api.Models
{
    public record SearchRequest(
        string SearchTerm = "",
        PartyType? Type = null,
        SanctionsStatus? SanctionsStatus = null,
        int Page = 1,
        int PageSize = 10
    );
}