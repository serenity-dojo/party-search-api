using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;

namespace PartySearchApi.Api.Services
{
    public class PartySearchService : IPartySearchService
    {
        private readonly IPartyRepository _repository;

        public PartySearchService(IPartyRepository repository)
        {
            _repository = repository;
        }

        public async Task<SearchResponse> SearchPartiesAsync(SearchRequest request)
        {
            var (parties, totalCount) = await _repository.SearchPartiesAsync(
                request.SearchTerm,
                request.Type,
                request.SanctionsStatus,
                request.Page,
                request.PageSize);

            // Calculate total pages (ceiling of totalCount / pageSize)
            int totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new SearchResponse(parties,
                                      new Pagination(totalCount, totalPages, request.Page, request.PageSize));
        }

        public async Task SeedTestDataAsync(List<Party> parties)
        {
            await _repository.AddPartiesAsync(parties);
        }
    }
}