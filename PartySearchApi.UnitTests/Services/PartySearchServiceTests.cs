using FluentAssertions;
using Moq;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PartySearchApi.UnitTests.Services
{
    public class PartySearchServiceTests
    {
        private readonly Mock<IPartyRepository> _mockRepository;
        private readonly IPartyService _service;

        public PartySearchServiceTests()
        {
            _mockRepository = new Mock<IPartyRepository>();
            _service = new PartyService(_mockRepository.Object);
        }

        [Fact]
        public async Task SearchPartiesAsync_BasicSearch_ShouldReturnFormattedSearchResponse()
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

        [Fact]
        public async Task SearchPartiesAsync_MultiplePages_ShouldCalculateTotalPagesCorrectly()
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

        [Fact]
        public async Task SearchPartiesAsync_EmptyResults_ShouldReturnEmptyResponseWithZeroPagination()
        {
            // Arrange
            var request = new SearchRequest
            {
                SearchTerm = "NonExistent",
                Page = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.SearchPartiesAsync(
                    "NonExistent", null, null, 1, 10))
                .ReturnsAsync((new List<Party>(), 0));

            // Act
            var result = await _service.SearchPartiesAsync(request);

            // Assert
            result.Results.Should().BeEmpty();
            result.Pagination.TotalResults.Should().Be(0);
            result.Pagination.TotalPages.Should().Be(0);
            result.Pagination.CurrentPage.Should().Be(1);
            result.Pagination.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task SearchPartiesAsync_WithFilters_ShouldPassFiltersToRepository()
        {
            // Arrange
            var request = new SearchRequest
            {
                SearchTerm = "Filter",
                Page = 1,
                PageSize = 10,
                Type = PartyType.Individual,
                SanctionsStatus = SanctionsStatus.PendingReview
            };

            _mockRepository.Setup(r => r.SearchPartiesAsync(
                    "Filter", PartyType.Individual, SanctionsStatus.PendingReview, 1, 10))
                .ReturnsAsync((new List<Party>(), 0));

            // Act
            await _service.SearchPartiesAsync(request);

            // Assert
            _mockRepository.Verify(r => r.SearchPartiesAsync(
                "Filter", PartyType.Individual, SanctionsStatus.PendingReview, 1, 10),
                Times.Once);
        }

        [Fact]
        public async Task SearchPartiesAsync_RequestingSpecificPage_ShouldRetrieveCorrectPage()
        {
            // Arrange
            var testParties = new List<Party>
            {
                new() {
                    PartyId = "P30000001",
                    Name = "Page Three Party 1",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.75m
                }
            };

            var request = new SearchRequest
            {
                SearchTerm = "Page",
                Page = 3,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.SearchPartiesAsync(
                    "Page", null, null, 3, 10))
                .ReturnsAsync((testParties, 25)); // 25 total results

            // Act
            var result = await _service.SearchPartiesAsync(request);

            // Assert
            result.Results.Should().HaveCount(1);
            result.Results[0].PartyId.Should().Be("P30000001");
            result.Pagination.CurrentPage.Should().Be(3);
            result.Pagination.TotalPages.Should().Be(3); // Ceiling of 25/10 = 3

            // Verify repository was called with the correct page number
            _mockRepository.Verify(r => r.SearchPartiesAsync(
                "Page", null, null, 3, 10),
                Times.Once);
        }

        [Fact]
        public async Task SearchPartiesAsync_WithPageSizeOverride_ShouldRespectRequestedPageSize()
        {
            // Arrange
            var testParties = new List<Party>
            {
                new() { PartyId = "P00000001", Name = "Custom PageSize Party 1" },
                new() { PartyId = "P00000002", Name = "Custom PageSize Party 2" },
                new() { PartyId = "P00000003", Name = "Custom PageSize Party 3" }
            };

            var request = new SearchRequest
            {
                SearchTerm = "Custom",
                Page = 1,
                PageSize = 3 // Custom page size
            };

            _mockRepository.Setup(r => r.SearchPartiesAsync(
                    "Custom", null, null, 1, 3))
                .ReturnsAsync((testParties, 10));

            // Act
            var result = await _service.SearchPartiesAsync(request);

            // Assert
            result.Results.Should().HaveCount(3);
            result.Pagination.PageSize.Should().Be(3);
            result.Pagination.TotalPages.Should().Be(4); // Ceiling of 10/3 = 4

            // Verify repository was called with the correct page size
            _mockRepository.Verify(r => r.SearchPartiesAsync(
                "Custom", null, null, 1, 3),
                Times.Once);
        }

    }
}