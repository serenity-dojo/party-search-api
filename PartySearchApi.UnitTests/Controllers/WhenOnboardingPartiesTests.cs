using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PartySearchApi.Api.Controllers;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Services;
using Xunit;

namespace PartySearchApi.UnitTests.Controllers
{
    public class WhenOnboardingPartiesTests
    {
        private readonly Mock<IPartyService> _partyService;
        private readonly PartiesController _controller;

        public WhenOnboardingPartiesTests()
        {
            _partyService = new Mock<IPartyService>();
            _controller = new PartiesController(_partyService.Object);
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

            _partyService
                .Setup(s => s.OnboardPartyAsync(It.IsAny<Party>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddParty(newParty);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(_controller.Search));
            createdResult.Value.Should().BeEquivalentTo(newParty);
        }

        [Fact]
        public async Task AddParty_NullParty_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.AddParty(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Party cannot be null.");
        }

        [Fact]
        public async Task AddParty_DuplicateParty_ReturnsBadRequest()
        {
            // Arrange
            var duplicateParty = new Party
            {
                PartyId = "P12345678",
                Name = "Duplicate Party",
                Type = PartyType.Organization,
                SanctionsStatus = SanctionsStatus.Approved,
                MatchScore = 0.95m
            };

            _partyService
                .Setup(s => s.OnboardPartyAsync(It.IsAny<Party>()))
                .ThrowsAsync(new InvalidOperationException("A party with ID 'P12345678' already exists."));

            // Act
            var result = await _controller.AddParty(duplicateParty);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("A party with ID 'P12345678' already exists.");
        }
    }
}

