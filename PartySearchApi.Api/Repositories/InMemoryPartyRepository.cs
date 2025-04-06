using PartySearchApi.Api.Models;

namespace PartySearchApi.Api.Repositories
{
    public class InMemoryPartyRepository : IPartyRepository
    {
        private readonly List<Party> _parties = new();

        public Task<List<Party>> GetAllPartiesAsync()
        {
            return Task.FromResult(_parties.ToList());
        }

        public Task AddPartiesAsync(List<Party> parties)
        {
            _parties.AddRange(parties);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync()
        {
            _parties.Clear();
            return Task.CompletedTask;
        }

        public Task<(List<Party> Parties, int TotalCount)> SearchPartiesAsync(
            string searchTerm,
            PartyType? type,
            SanctionsStatus? sanctionsStatus,
            int page = 1,
            int pageSize = 10)
        {
            // Apply all filters first
            var query = _parties.AsQueryable();

            // Filter by search term (case insensitive)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.PartyId.ToLower().Contains(searchTerm));
            }

            // Filter by type if provided
            if (type != null)
            {
                query = query.Where(p => p.Type == type);
            }

            // Filter by sanctions status if provided
            if (sanctionsStatus != null)
            {
                query = query.Where(p => p.SanctionsStatus == sanctionsStatus);
            }

            // Order by name (alphabetical)
            query = query.OrderBy(p => p.Name);

            // Get total count before pagination
            int totalCount = query.Count();

            // Apply pagination
            var paginatedResults = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((paginatedResults, totalCount));
        }
    }
}