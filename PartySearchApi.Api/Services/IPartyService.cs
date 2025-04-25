using PartySearchApi.Api.Models;

namespace PartySearchApi.Api.Services
{
    public interface IPartyService
    {
        Task<SearchResponse> SearchPartiesAsync(SearchRequest request);
        Task OnboardPartyAsync(Party party);

        Task SeedTestDataAsync(List<Party> parties);
    }
}