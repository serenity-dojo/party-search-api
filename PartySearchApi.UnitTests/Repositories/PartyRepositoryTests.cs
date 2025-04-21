using FluentAssertions;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;

namespace PartySearchApi.UnitTests.Repositories
{
    [TestFixture]
    public class PartyRepositoryTests
    {
        private IPartyRepository _repository;
        private List<Party> _testParties;

        [SetUp]
        public async Task Setup()
        {
            // Create our test repository
            _repository = new InMemoryPartyRepository();

            // Set up test data
            _testParties =
            [
                new() {
                    PartyId = "P12345678",
                    Name = "Acme Corporation",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.95m
                },
                new() {
                    PartyId = "P87654321",
                    Name = "Acme Inc.",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.PendingReview,
                    MatchScore = 0.65m
                },
                new() {
                    PartyId = "P87654329",
                    Name = "Axe Capital",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Escalated,
                    MatchScore = 0.85m
                }
            ];

            await _repository.AddPartiesAsync(_testParties);
        }

        [Test]
        public async Task SearchPartiesAsync_SearchByFullName_ReturnsExactMatch()
        {
            // Arrange & Act
            var (Parties, _) = await _repository.SearchPartiesAsync("Acme Corporation");

            // Assert
            _ = Parties.Should().HaveCount(1);
            _ = Parties[0].PartyId.Should().Be("P12345678");
            _ = Parties[0].Name.Should().Be("Acme Corporation");
        }

        [Test]
        public async Task SearchPartiesAsync_SearchByPartialName_ReturnsAllMatches()
        {
            // Arrange & Act
            var (Parties, TotalCount) = await _repository.SearchPartiesAsync("Acme");

            // Assert
            _ = Parties.Should().HaveCount(2);
            _ = Parties.Should().Contain(p => p.PartyId == "P12345678");
            _ = Parties.Should().Contain(p => p.PartyId == "P87654321");
        }

        [Test]
        public async Task SearchPartiesAsync_SearchByPartialID_ReturnsMatchingParties()
        {
            // Arrange & Act
            var (Parties, TotalCount) = await _repository.SearchPartiesAsync("P87654");

            // Assert
            _ = Parties.Should().HaveCount(2);
            _ = Parties.Should().Contain(p => p.PartyId == "P87654321");
            _ = Parties.Should().Contain(p => p.PartyId == "P87654329");
        }

        [Test]
        public async Task SearchPartiesAsync_FilterByType_ReturnsOnlyMatchingType()
        {
            // First add an individual
            await _repository.AddPartiesAsync(
            [
                new() {
                    PartyId = "P11111111",
                    Name = "John Acme",
                    Type = PartyType.Individual,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.90m
                }
            ]);

            // Arrange & Act
            var (Parties, TotalCount) = await _repository.SearchPartiesAsync("Acme", type: PartyType.Organization);

            // Assert
            _ = Parties.Should().HaveCount(2);
            _ = Parties.Should().OnlyContain(p => p.Type == PartyType.Organization);
        }

        [Test]
        public async Task SearchPartiesAsync_FilterBySanctionsStatus_ReturnsOnlyMatchingStatus()
        {
            // Arrange & Act
            var (Parties, _) = await _repository.SearchPartiesAsync("Acme", sanctionsStatus: SanctionsStatus.PendingReview);

            // Assert
            _ = Parties.Should().HaveCount(1);
            _ = Parties[0].PartyId.Should().Be("P87654321");
            _ = Parties[0].SanctionsStatus.Should().Be(SanctionsStatus.PendingReview);
        }

    }
}