using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;

namespace PartySearchApi.Api.Services
{
    /// <summary>
    /// Service for managing party-related operations, including searching, onboarding, and seeding test data.
    /// </summary>
    public class PartyService(IPartyRepository repository) : IPartyService
    {
        private readonly IPartyRepository _repository = repository;

        /// <summary>
        /// Searches for parties in the repository based on the provided search request.
        /// </summary>
        /// <param name="request">The search request containing search term, filters, and pagination details.</param>
        /// <returns>A <see cref="SearchResponse"/> containing the matching parties and pagination metadata.</returns>
        public async Task<SearchResponse> SearchPartiesAsync(SearchRequest request)
        {
            Console.WriteLine($"Search request: {request.SearchTerm}, Page: {request.Page}");

            var (parties, totalCount) = await _repository.SearchPartiesAsync(
                request.SearchTerm,
                request.Type,
                request.SanctionsStatus,
                request.Page,
                request.PageSize);

            // Calculate total pages (ceiling of totalCount / pageSize)
            int totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            string message = (parties.Count == 0) ? $"No parties found matching '{request.SearchTerm}'" : "";

            return new SearchResponse(parties,
                                      new Pagination(totalCount, totalPages, request.Page, request.PageSize),
                                      message);
        }

        /// <summary>
        /// Seeds the repository with a list of test parties.
        /// </summary>
        /// <param name="parties">The list of parties to add to the repository.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SeedTestDataAsync(List<Party> parties)
        {
            await _repository.AddPartiesAsync(parties);
        }

        /// <summary>
        /// Onboards a new party into the repository.
        /// </summary>
        /// <param name="party">The party to be onboarded.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided party is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a party with the same ID already exists in the repository.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnboardPartyAsync(Party party)
        {
            if (party == null)
            {
                throw new ArgumentNullException(nameof(party), "Party cannot be null.");
            }
            // Write a log message indicating the party being onboarded
            Console.WriteLine($"Onboarding party: {party.Name}, Type: {party.Type}, SanctionsStatus: {party.SanctionsStatus}");

            // if the partyId is not provided, generate a random party ID in the format P-12345678
            if (party.PartyId == null || party.PartyId.Length == 0)
            {
                party.PartyId = "P-" + new Random().Next(10000000, 100000000).ToString();
                Console.WriteLine($"Party ID: {party.PartyId}");
            }

            var existingParties = await _repository.GetAllPartiesAsync();
            if (existingParties.Any(p => p.PartyId == party.PartyId))
            {
                Console.WriteLine($"Error: A party with ID '{party.PartyId}' already exists.");
                throw new InvalidOperationException($"A party with ID '{party.PartyId}' already exists.");
            }

            await _repository.AddPartiesAsync(new List<Party> { party });
            Console.WriteLine($"Party onboarded successfully: {party.PartyId} - {party.Name}");
        }
    }
}
