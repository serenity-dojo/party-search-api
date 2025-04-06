using PartySearchApi.Api.Models;

namespace PartySearchApi.Api.Services
{
    public interface IPartySearchService
    {
        Task<SearchResponse> SearchPartiesAsync(SearchRequest request);
        Task SeedTestDataAsync(List<Party> parties);
    }
}