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
            _testParties = new List<Party>
            {
                new Party {
                    PartyId = "P12345678",
                    Name = "Acme Corporation",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "95%"
                },
                new Party {
                    PartyId = "P87654321",
                    Name = "Acme Inc.",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.PendingReview,
                    MatchScore = "65%"
                },
                new Party {
                    PartyId = "P87654329",
                    Name = "Axe Capital",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Escalated,
                    MatchScore = "85%"
                }
            };

            await _repository.AddPartiesAsync(_testParties);
        }

        [Test]
        public async Task SearchPartiesAsync_SearchByFullName_ReturnsExactMatch()
        {
            // Arrange & Act
            var result = await _repository.SearchPartiesAsync("Acme Corporation");

            // Assert
            _ = result.Parties.Should().HaveCount(1);
            _ = result.Parties[0].PartyId.Should().Be("P12345678");
            _ = result.Parties[0].Name.Should().Be("Acme Corporation");
        }

        [Test]
        public async Task SearchPartiesAsync_SearchByPartialName_ReturnsAllMatches()
        {
            // Arrange & Act
            var result = await _repository.SearchPartiesAsync("Acme");

            // Assert
            _ = result.Parties.Should().HaveCount(2);
            _ = result.Parties.Should().Contain(p => p.PartyId == "P12345678");
            _ = result.Parties.Should().Contain(p => p.PartyId == "P87654321");
        }

        [Test]
        public async Task SearchPartiesAsync_SearchByPartialID_ReturnsMatchingParties()
        {
            // Arrange & Act
            var result = await _repository.SearchPartiesAsync("P87654");

            // Assert
            _ = result.Parties.Should().HaveCount(2);
            _ = result.Parties.Should().Contain(p => p.PartyId == "P87654321");
            _ = result.Parties.Should().Contain(p => p.PartyId == "P87654329");
        }

        [Test]
        public async Task SearchPartiesAsync_FilterByType_ReturnsOnlyMatchingType()
        {
            // First add an individual
            await _repository.AddPartiesAsync(new List<Party>
            {
                new Party {
                    PartyId = "P11111111",
                    Name = "John Acme",
                    Type = PartyType.Individual,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "90%"
                }
            });

            // Arrange & Act
            var result = await _repository.SearchPartiesAsync("Acme", type: PartyType.Organization);

            // Assert
            _ = result.Parties.Should().HaveCount(2);
            _ = result.Parties.Should().OnlyContain(p => p.Type == PartyType.Organization);
        }

        [Test]
        public async Task SearchPartiesAsync_FilterBySanctionsStatus_ReturnsOnlyMatchingStatus()
        {
            // Arrange & Act
            var result = await _repository.SearchPartiesAsync("Acme", sanctionsStatus: SanctionsStatus.PendingReview);

            // Assert
            _ = result.Parties.Should().HaveCount(1);
            _ = result.Parties[0].PartyId.Should().Be("P87654321");
            _ = result.Parties[0].SanctionsStatus.Should().Be(SanctionsStatus.PendingReview);
        }

        public async Task SearchPartiesAsync_Pagination_ReturnsCorrectPage()
        {
            // Add more parties to test pagination
            var moreParties = new List<Party>();
            for (int i = 1; i <= 20; i++)
            {
                moreParties.Add(new Party
                {
                    PartyId = $"P{i:00000000}",
                    Name = $"Test Party {i}",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "80%"
                });
            }
            await _repository.AddPartiesAsync(moreParties);

            // Arrange & Act
            var result = await _repository.SearchPartiesAsync("Test", page: 2, pageSize: 5);

            // Assert
            _ = result.Parties.Should().HaveCount(5);
            _ = result.TotalCount.Should().Be(20);  // Total matches

            var expectedItems = moreParties
                .OrderBy(p => p.Name)  // Match the repository's ordering
                .Skip(5)               // Skip the first page
                .Take(5);              // Take the second page

            _ = result.Parties[0].Name.Should().Be(expectedItems.First().Name);
        }
    }
}