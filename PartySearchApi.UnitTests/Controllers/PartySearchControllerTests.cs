using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PartySearchApi.Api.Controllers;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Services;

namespace PartySearchApi.UnitTests.Controllers
{
    [TestFixture]
    public class PartySearchControllerTests
    {
        private Mock<IPartySearchService> _mockService;
        private PartiesController _controller;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IPartySearchService>();
            _controller = new PartiesController(_mockService.Object);
        }

        [Test]
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
                        MatchScore = "95%"
                    }
                ],
                new Pagination(1, 1, 1, 10)
             );

            _ = _mockService.Setup(s => s.SearchPartiesAsync(It.Is<SearchRequest>(r =>
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
            _ = okResult.Should().NotBeNull();

            var returnedResponse = okResult.Value as SearchResponse;
            _ = returnedResponse.Should().NotBeNull();
            _ = returnedResponse.Data.Should().HaveCount(1);
            _ = returnedResponse.Data[0].PartyId.Should().Be("P12345678");
        }
    }
}