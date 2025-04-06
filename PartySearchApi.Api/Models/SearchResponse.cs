namespace PartySearchApi.Api.Models
{
    public record SearchResponse(List<Party> Data, Pagination Pagination);
    public record Pagination(int TotalResults, int TotalPages, int CurrentPage, int PageSize);

}