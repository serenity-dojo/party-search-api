using PartySearchApi.Api.Models;

namespace PartySearchApi.Api.Repositories
{
    public interface IPartyRepository
    {
        Task<(List<Party> Parties, int TotalCount)> SearchPartiesAsync(
            string searchTerm,
            PartyType? type = null,
            SanctionsStatus? sanctionsStatus = null,
            int page = 1,
            int pageSize = 10);

        Task<List<Party>> GetAllPartiesAsync();
        Task AddPartiesAsync(List<Party> parties);
        Task ClearAllAsync();
    }
}