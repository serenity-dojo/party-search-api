using FluentAssertions;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PartySearchApi.UnitTests.Repositories
{
    public class PartyRepositoryTests : IAsyncLifetime
    {
        private IPartyRepository _repository;
        private List<Party> _testParties;

        public async Task InitializeAsync()
        {
            // Create our test repository
            _repository = new InMemoryPartyRepository();

            // Set up test data
            _testParties =
            [
                new()
                {
                    PartyId = "P12345678",
                    Name = "Acme Corporation",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.95m
                },
                new()
                {
                    PartyId = "P87654321",
                    Name = "Acme Inc.",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.PendingReview,
                    MatchScore = 0.65m
                },
                new()
                {
                    PartyId = "P87654329",
                    Name = "Axe Capital",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Escalated,
                    MatchScore = 0.85m
                }
            ];

            await _repository.AddPartiesAsync(_testParties);
        }

        public Task DisposeAsync()
        {
            // Clean up resources if needed
            return Task.CompletedTask;
        }

        [Fact]
        public async Task SearchByFullName_ShouldReturnExactMatch()
        {
            // Arrange & Act
            var (parties, _) = await _repository.SearchPartiesAsync("Acme Corporation");

            // Assert
            parties.Should().HaveCount(1);
            parties[0].PartyId.Should().Be("P12345678");
            parties[0].Name.Should().Be("Acme Corporation");
        }

        [Fact]
        public async Task SearchByPartialName_ShouldReturnAllMatches()
        {
            // Arrange & Act
            var (parties, _) = await _repository.SearchPartiesAsync("Acme");

            // Assert
            parties.Should().HaveCount(2);
            parties.Should().Contain(p => p.PartyId == "P12345678");
            parties.Should().Contain(p => p.PartyId == "P87654321");
        }

        [Fact]
        public async Task SearchByPartialId_ShouldReturnMatchingParties()
        {
            // Arrange & Act
            var (parties, _) = await _repository.SearchPartiesAsync("P87654");

            // Assert
            parties.Should().HaveCount(2);
            parties.Should().Contain(p => p.PartyId == "P87654321");
            parties.Should().Contain(p => p.PartyId == "P87654329");
        }

        [Fact]
        public async Task FilterByType_ShouldReturnOnlyPartiesOfMatchingType()
        {
            // Arrange
            await _repository.AddPartiesAsync(
            [
                new()
                {
                    PartyId = "P11111111",
                    Name = "John Acme",
                    Type = PartyType.Individual,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.90m
                }
            ]);

            // Act
            var (parties, _) = await _repository.SearchPartiesAsync("Acme", type: PartyType.Organization);

            // Assert
            parties.Should().HaveCount(2);
            parties.Should().OnlyContain(p => p.Type == PartyType.Organization);
        }

        [Fact]
        public async Task FilterBySanctionsStatus_ShouldReturnOnlyPartiesWithMatchingStatus()
        {
            // Arrange & Act
            var (parties, _) = await _repository.SearchPartiesAsync("Acme", sanctionsStatus: SanctionsStatus.PendingReview);

            // Assert
            parties.Should().HaveCount(1);
            parties[0].PartyId.Should().Be("P87654321");
            parties[0].SanctionsStatus.Should().Be(SanctionsStatus.PendingReview);
        }
    }
}