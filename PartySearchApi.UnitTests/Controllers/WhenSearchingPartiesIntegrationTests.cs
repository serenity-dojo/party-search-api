using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PartySearchApi.Api.Controllers;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using Xunit;

namespace PartySearchApi.UnitTests.Controllers
{
    public class WhenSearchingPartiesIntegrationTests
    {
        private readonly IPartyRepository _partyRepository;
        private readonly IPartyService _partyService;
        private readonly PartiesController _controller;

        public WhenSearchingPartiesIntegrationTests()
        {
            _partyRepository = new InMemoryPartyRepository();
            _partyService = new PartyService(_partyRepository);
            _controller = new PartiesController(_partyService);
        }


        [Fact]
        public async Task Search_Party_For_Existing_Party()
        {
            // Arrange
            var newParty = new Party
            {
                PartyId = "P12345678",
                Name = "Test Party",
                Type = PartyType.Organization,
                SanctionsStatus = SanctionsStatus.Approved,
                MatchScore = 0.95m
            };
            await _controller.AddParty(newParty);

            var result = await _controller.Search("Test Party", null, null, "1", "10");

            // Assert that the result is the correct party

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeOfType<SearchResponse>();
            var searchResponse = okResult.Value as SearchResponse;
            searchResponse!.Results.Should().ContainSingle(p => p.PartyId == "P12345678");
            searchResponse.Results.First().Name.Should().Be("Test Party");
            searchResponse.Results.First().Type.Should().Be(PartyType.Organization);
            searchResponse.Results.First().SanctionsStatus.Should().Be(SanctionsStatus.Approved);
            searchResponse.Results.First().MatchScore.Should().Be(0.95m);
        }


        [Fact]
        public async Task AddParty_ValidParty_ReturnsCreatedResult()
        {
            // Arrange
            var newParty = new Party
            {
                PartyId = "P12345678",
                Name = "Test Party",
                Type = PartyType.Organization,
                SanctionsStatus = SanctionsStatus.Approved,
                MatchScore = 0.95m
            };

            // Act
            var result = await _controller.AddParty(newParty);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(_controller.Search));
            createdResult.Value.Should().BeEquivalentTo(newParty);
        }

    }
}

