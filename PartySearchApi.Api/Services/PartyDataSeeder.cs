// Create this in a new file: PartySearchApi.Api/Services/PartyDataSeeder.cs
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PartySearchApi.Api.Services
{
    public class PartyDataSeeder(IPartyRepository repository, ILogger<PartyDataSeeder> logger)
    {
        private readonly IPartyRepository _repository = repository;
        private readonly ILogger<PartyDataSeeder> _logger = logger;

        public async Task<bool> SeedFromJsonFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Seed data file not found at path: {FilePath}", filePath);
                    return false;
                }

                string jsonData = await File.ReadAllTextAsync(filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                var parties = JsonSerializer.Deserialize<List<Party>>(jsonData, options);

                if (parties == null || parties.Count == 0)
                {
                    _logger.LogWarning("No parties found in the seed data file.");
                    return false;
                }

                // Check if repository already has data
                var existingParties = await _repository.GetAllPartiesAsync();
                if (existingParties.Count != 0)
                {
                    _logger.LogInformation("Database already contains data. Skipping seed operation.");
                    return false;
                }

                await _repository.AddPartiesAsync(parties);
                _logger.LogInformation("Successfully seeded database with {Count} parties from {FilePath}",
                    parties.Count, filePath);
                return true;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error parsing JSON data");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data");
                return false;
            }
        }

        // This overload is useful for testing where we want to directly provide JSON content
        public async Task<bool> SeedFromJsonString(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                var parties = JsonSerializer.Deserialize<List<Party>>(jsonContent, options);

                if (parties == null || parties.Count == 0)
                {
                    _logger.LogWarning("No parties found in the provided JSON.");
                    return false;
                }

                // Check if repository already has data
                var existingParties = await _repository.GetAllPartiesAsync();
                if (existingParties.Count != 0)
                {
                    _logger.LogInformation("Database already contains data. Skipping seed operation.");
                    return false;
                }

                await _repository.AddPartiesAsync(parties);
                _logger.LogInformation("Successfully seeded database with {Count} parties", parties.Count);
                return true;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error parsing JSON data");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data");
                return false;
            }
        }
    }
}