using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PartySearchApi.Api.Controllers;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Services;
using Xunit;

namespace PartySearchApi.UnitTests.Controllers
{
    public class WhenSearchingPartiesTests
    {
        private readonly Mock<IPartyService> _partyService;
        private readonly PartiesController _controller;

        public WhenSearchingPartiesTests()
        {
            _partyService = new Mock<IPartyService>();
            _controller = new PartiesController(_partyService.Object);  
        }

        [Fact]
        public async Task Search_ReturnsOkObjectResult_WithSearchResponse()
        {
            // Arrange
            var expectedRequest = new SearchRequest
            {
                SearchTerm = "Acme",
                Type = PartyType.Organization,
                Page = 1,
                PageSize = 10
            };

            var response = new SearchResponse(
                [
                    new() {
                        PartyId = "P12345678",
                        Name = "Acme Corporation",
                        Type = PartyType.Organization,
                        SanctionsStatus = SanctionsStatus.Approved,
                        MatchScore = 0.95m
                    }
                ],
                new Pagination(1, 1, 1, 10)
             );

            _partyService.Setup(s => s.SearchPartiesAsync(It.Is<SearchRequest>(r =>
                r.SearchTerm == expectedRequest.SearchTerm &&
                r.Type == expectedRequest.Type &&
                r.Page == expectedRequest.Page &&
                r.PageSize == expectedRequest.PageSize)))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Search(
                "Acme",
                "Organization",
                null,
                "1",
                "10");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedResponse = okResult.Value as SearchResponse;
            returnedResponse.Should().NotBeNull();
            returnedResponse.Results.Should().HaveCount(1);
            returnedResponse.Results[0].PartyId.Should().Be("P12345678");
        }
    }
}