using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;

namespace PartySearchApi.Api.Services
{
    public class PartySearchService(IPartyRepository repository) : IPartySearchService
    {
        private readonly IPartyRepository _repository = repository;

        public async Task<SearchResponse> SearchPartiesAsync(SearchRequest request)
        {
            Console.WriteLine($"Search request: {request.SearchTerm}, Page: {request.Page}");
            
            var (parties, totalCount) = await _repository.SearchPartiesAsync(
                request.SearchTerm,
                request.Type,
                request.SanctionsStatus,
                request.Page,
                request.PageSize);

            Console.WriteLine("Matching parties: " + string.Join(", ", parties.Select(p => p.Name)));

            // Calculate total pages (ceiling of totalCount / pageSize)
            int totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            string message = (parties.Count == 0) ? $"No parties found matching '{request.SearchTerm}'" : "";

            return new SearchResponse(parties,
                                      new Pagination(totalCount, totalPages, request.Page, request.PageSize),
                                      message);
        }

        public async Task SeedTestDataAsync(List<Party> parties)
        {
            await _repository.AddPartiesAsync(parties);
        }
    }
}