using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PartySearchApi.UnitTests.Services
{
    public class PartyDataSeederTests : IDisposable
    {
        private readonly Mock<IPartyRepository> _mockRepository;
        private readonly Mock<ILogger<PartyDataSeeder>> _mockLogger;
        private readonly PartyDataSeeder _seeder;
        private readonly string _testFilesPath;

        public PartyDataSeederTests()
        {
            _mockRepository = new Mock<IPartyRepository>();
            _mockLogger = new Mock<ILogger<PartyDataSeeder>>();
            _seeder = new PartyDataSeeder(_mockRepository.Object, _mockLogger.Object);

            // Create directory for test files in the current directory
            _testFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");
            if (!Directory.Exists(_testFilesPath))
            {
                Directory.CreateDirectory(_testFilesPath);
            }
        }

        public void Dispose()
        {
            // Clean up test files after tests run
            if (Directory.Exists(_testFilesPath))
            {
                Directory.Delete(_testFilesPath, true);
            }
        }

        [Fact]
        public async Task SeedFromJsonFile_ValidJson_ShouldSeedDataAndReturnTrue()
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

        [Fact]
        public async Task SeedFromJsonFile_EmptyJson_ShouldNotSeedAndReturnFalse()
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
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No parties found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SeedFromJsonFile_InvalidJson_ShouldNotSeedAndReturnFalse()
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
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error parsing")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }


        [Fact]
        public async Task SeedFromJsonFile_RepositoryAlreadyHasData_ShouldNotSeedAndReturnFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllPartiesAsync())
                .ReturnsAsync([new Party()]);

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
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("already contains data")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }


        [Fact]
        public async Task SeedFromJsonFile_RepositoryThrowsException_ShouldHandleAndReturnFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllPartiesAsync())
                .ReturnsAsync([]);
            _mockRepository.Setup(r => r.AddPartiesAsync(It.IsAny<List<Party>>()))
                .ThrowsAsync(new Exception("Database connection error"));

            string validJson = @"[
                {
                    ""partyId"": ""P12345678"",
                    ""name"": ""Acme Corporation"",
                    ""type"": ""Organization"",
                    ""sanctionsStatus"": ""Approved"",
                    ""matchScore"": 0.95
                }
            ]";

            string filePath = Path.Combine(_testFilesPath, "throws-exception.json");
            await File.WriteAllTextAsync(filePath, validJson);

            // Act
            var result = await _seeder.SeedFromJsonFile(filePath);

            // Assert
            result.Should().BeFalse();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error seeding")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

    }
}