namespace PartySearchApi.Api.Models
{
    public record SearchResponse(List<Party> Results, Pagination Pagination, String Message = "");
    public record Pagination(int TotalResults, int TotalPages, int CurrentPage, int PageSize);

}