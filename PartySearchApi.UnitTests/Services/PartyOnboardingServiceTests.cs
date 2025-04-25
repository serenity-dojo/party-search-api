using FluentAssertions;
using Moq;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert; 

namespace PartySearchApi.UnitTests.Services
{
    public class PartyOnboardingServiceTests
    {
        private readonly Mock<IPartyRepository> _mockRepository;
        private readonly PartyService _service;

        public PartyOnboardingServiceTests()
        {
            _mockRepository = new Mock<IPartyRepository>();
            _service = new PartyService(_mockRepository.Object);
        }

        [Fact]
        public async Task OnboardPartyAsync_ShouldAddValidPartyToRepository()
        {
            // Arrange
            var party = new Party
            {
                PartyId = "P12345678",
                Name = "Acme Corporation",
                Type = PartyType.Organization,
                SanctionsStatus = SanctionsStatus.Approved,
                MatchScore = 0.95m
            };

            _mockRepository.Setup(r => r.GetAllPartiesAsync())
                .ReturnsAsync(new List<Party>());

            // Act
            await _service.OnboardPartyAsync(party);

            // Assert
            _mockRepository.Verify(r => r.AddPartiesAsync(It.Is<List<Party>>(
                p => p.Count == 1 && p[0].PartyId == "P12345678")), Times.Once);
        }

        [Fact]
        public async Task OnboardPartyAsync_ShouldThrowException_WhenPartyIsNull()
        {
            // Arrange
            Party nullParty = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.OnboardPartyAsync(nullParty));

            exception.ParamName.Should().Be("party");
            exception.Message.Should().Contain("Party cannot be null");

            // Verify repository was never called
            _mockRepository.Verify(r => r.GetAllPartiesAsync(), Times.Never);
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Fact]
        public async Task OnboardPartyAsync_ShouldThrowException_WhenPartyWithSameIdAlreadyExists()
        {
            // Arrange
            var existingPartyId = "P12345678";

            var newParty = new Party
            {
                PartyId = existingPartyId,
                Name = "New Party",
                Type = PartyType.Organization,
                SanctionsStatus = SanctionsStatus.Approved,
                MatchScore = 0.85m
            };

            var existingParties = new List<Party>
            {
                new Party
                {
                    PartyId = existingPartyId,
                    Name = "Existing Party",
                    Type = PartyType.Individual,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = 0.90m
                }
            };

            _mockRepository.Setup(r => r.GetAllPartiesAsync())
                .ReturnsAsync(existingParties);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.OnboardPartyAsync(newParty));

            exception.Message.Should().Be($"A party with ID '{existingPartyId}' already exists.");

            // Verify repository was called to check existence but not to add
            _mockRepository.Verify(r => r.GetAllPartiesAsync(), Times.Once);
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }
    }
}