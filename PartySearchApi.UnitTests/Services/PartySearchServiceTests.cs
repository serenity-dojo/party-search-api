using FluentAssertions;
using Moq;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;

namespace PartySearchApi.UnitTests.Services
{
    [TestFixture]
    public class PartySearchServiceTests
    {
        private Mock<IPartyRepository> _mockRepository;
        private IPartySearchService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IPartyRepository>();
            _service = new PartySearchService(_mockRepository.Object);
        }

        [Test]
        public async Task SearchPartiesAsync_ReturnsFormattedSearchResponse()
        {
            // Arrange
            var testParties = new List<Party>
            {
                new() {
                    PartyId = "P12345678",
                    Name = "Acme Corporation",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.95m
                }
            };

            var request = new SearchRequest
            {
                SearchTerm = "Acme",
                Page = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.SearchPartiesAsync(
                    "Acme", null, null, 1, 10))
                .ReturnsAsync((testParties, 1));

            // Act
            var result = await _service.SearchPartiesAsync(request);

            // Assert
            result.Results.Should().HaveCount(1);
            result.Results[0].PartyId.Should().Be("P12345678");

            result.Pagination.TotalResults.Should().Be(1);
            result.Pagination.TotalPages.Should().Be(1);
            result.Pagination.CurrentPage.Should().Be(1);
            result.Pagination.PageSize.Should().Be(10);
        }

        [Test]
        public async Task SearchPartiesAsync_CalculatesTotalPagesCorrectly()
        {
            // Arrange
            var testParties = new List<Party>();
            for (int i = 1; i <= 10; i++)
            {
                testParties.Add(new Party
                {
                    PartyId = $"P{i:00000000}",
                    Name = $"Test Party {i}",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.80m
                });
            }

            var request = new SearchRequest
            {
                SearchTerm = "Test",
                Page = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.SearchPartiesAsync(
                    "Test", null, null, 1, 10))
                .ReturnsAsync((testParties, 95)); // 95 total results

            // Act
            var result = await _service.SearchPartiesAsync(request);

            // Assert
            result.Pagination.TotalResults.Should().Be(95);
            result.Pagination.TotalPages.Should().Be(10); // Ceiling of 95/10 = 10
        }
    }
}