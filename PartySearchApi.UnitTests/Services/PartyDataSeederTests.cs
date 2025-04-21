// In PartySearchApi.UnitTests/Services/PartyDataSeederTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PartySearchApi.UnitTests.Services
{
    [TestFixture]
    public class PartyDataSeederTests
    {
        private Mock<IPartyRepository> _mockRepository;
        private Mock<ILogger<PartyDataSeeder>> _mockLogger;
        private PartyDataSeeder _seeder;
        private string _testFilesPath;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IPartyRepository>();
            _mockLogger = new Mock<ILogger<PartyDataSeeder>>();
            _seeder = new PartyDataSeeder(_mockRepository.Object, _mockLogger.Object);

            // Create directory for test files in the output directory
            _testFilesPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
            if (!Directory.Exists(_testFilesPath))
            {
                Directory.CreateDirectory(_testFilesPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test files after tests run
            if (Directory.Exists(_testFilesPath))
            {
                Directory.Delete(_testFilesPath, true);
            }
        }

        [Test]
        public async Task SeedFromJsonFile_ValidJson_ReturnsTrue()
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
                    ""matchScore"": 0.95
                },
                {
                    ""partyId"": ""P87654321"",
                    ""name"": ""John Smith"",
                    ""type"": ""Individual"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": 0.90
                }
            ]";

            string filePath = Path.Combine(_testFilesPath, "valid-parties.json");
            await File.WriteAllTextAsync(filePath, validJson);

            // Act
            var result = await _seeder.SeedFromJsonFile(filePath);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.Is<List<Party>>(p => p.Count == 2)), Times.Once);
        }

        [Test]
        public async Task SeedFromJsonFile_EmptyJson_ReturnsFalse()
        {
            // Arrange
            string emptyJson = "[]";
            string filePath = Path.Combine(_testFilesPath, "empty-parties.json");
            await File.WriteAllTextAsync(filePath, emptyJson);

            // Act
            var result = await _seeder.SeedFromJsonFile(filePath);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonFile_InvalidJson_ReturnsFalse()
        {
            // Arrange
            string invalidJson = "This is not JSON";
            string filePath = Path.Combine(_testFilesPath, "invalid-parties.json");
            await File.WriteAllTextAsync(filePath, invalidJson);

            // Act
            var result = await _seeder.SeedFromJsonFile(filePath);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonFile_MissingRequiredFields_ReturnsFalse()
        {
            // Arrange
            string invalidJson = @"[
                {
                    ""name"": ""Missing Party ID"",
                    ""type"": ""Organization"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": 0.95
                }
            ]";

            string filePath = Path.Combine(_testFilesPath, "missing-fields-parties.json");
            await File.WriteAllTextAsync(filePath, invalidJson);

            // Act
            var result = await _seeder.SeedFromJsonFile(filePath);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonFile_RepositoryAlreadyHasData_ReturnsFalse()
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
                    ""matchScore"": 0.95
                }
            ]";

            string filePath = Path.Combine(_testFilesPath, "repo-already-has-data.json");
            await File.WriteAllTextAsync(filePath, validJson);

            // Act
            var result = await _seeder.SeedFromJsonFile(filePath);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }

        [Test]
        public async Task SeedFromJsonFile_FileNotFound_ReturnsFalse()
        {
            // Arrange
            string nonExistentFilePath = Path.Combine(_testFilesPath, "non-existent-file.json");

            // Act
            var result = await _seeder.SeedFromJsonFile(nonExistentFilePath);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.AddPartiesAsync(It.IsAny<List<Party>>()), Times.Never);
        }
    }
}