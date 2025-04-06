// In PartySearchApi.UnitTests/Services/PartyDataSeederTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartySearchApi.UnitTests.Services
{
    [TestFixture]
    public class PartyDataSeederTests
    {
        private Mock<IPartyRepository> _mockRepository;
        private Mock<ILogger<PartyDataSeeder>> _mockLogger;
        private PartyDataSeeder _seeder;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IPartyRepository>();
            _mockLogger = new Mock<ILogger<PartyDataSeeder>>();
            _seeder = new PartyDataSeeder(_mockRepository.Object, _mockLogger.Object);
        }

        [Test]
        public async Task SeedFromJsonString_ValidJson_ReturnsTrue()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllPartiesAsync())
                .ReturnsAsync([]);

            string validJson = @"[
                {
                    ""partyId"": ""P12345678"",
                    ""name"": ""Acme Corporation"",
                    ""type"": ""Organization"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": ""95%""
                },
                {
                    ""partyId"": ""P87654321"",
                    ""name"": ""John Smith"",
                    ""type"": ""Individual"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": ""90%""
                }
            ]";

            // Act
            var result = await _seeder.SeedFromJsonString(validJson);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.Is<List<Party>>(p => p.Count == 2)), Times.Once);
        }

        [Test]
        public async Task SeedFromJsonString_EmptyJson_ReturnsFalse()
        {
            // Arrange
            string emptyJson = "[]";

            // Act
            var result = await _seeder.SeedFromJsonString(emptyJson);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonString_InvalidJson_ReturnsFalse()
        {
            // Arrange
            string invalidJson = "This is not JSON";

            // Act
            var result = await _seeder.SeedFromJsonString(invalidJson);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonString_MissingRequiredFields_ReturnsFalse()
        {
            // Arrange
            string invalidJson = @"[
                {
                    ""name"": ""Missing Party ID"",
                    ""type"": ""Organization"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": ""95%""
                }
            ]";

            // Act
            var result = await _seeder.SeedFromJsonString(invalidJson);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonString_RepositoryAlreadyHasData_ReturnsFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllPartiesAsync())
                .ReturnsAsync([new()]);

            string validJson = @"[
                {
                    ""partyId"": ""P12345678"",
                    ""name"": ""Acme Corporation"",
                    ""type"": ""Organization"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": ""95%""
                }
            ]";

            // Act
            var result = await _seeder.SeedFromJsonString(validJson);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }
    }
}