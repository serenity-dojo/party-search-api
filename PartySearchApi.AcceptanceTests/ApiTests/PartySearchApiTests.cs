using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;

namespace PartySearchApi.AcceptanceTests.ApiTests
{
    [TestFixture]
    public class PartySearchApiTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _httpClient;
        private readonly InMemoryPartyRepository _repository = new();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [SetUp]
        public async Task Setup()
        {
            // Use WebApplicationFactory which supports minimal hosting model
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing repository registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IPartyRepository));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add our test repository instance
                        services.AddSingleton<IPartyRepository>(_repository);
                    });
                });

            // Get client from factory
            _httpClient = _factory.CreateClient();

            Console.WriteLine($"Test client BaseAddress: {_httpClient.BaseAddress}");

            // Clear repository and seed standard test data
            await _repository.ClearAllAsync();
            await SeedTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task BasicConnectivityTest()
        {
            // Make a simple request
            var response = await _httpClient.GetAsync("api/partysearch/search?searchTerm=Acme");

            Console.WriteLine($"Response status: {response.StatusCode}");

            // Only read content if successful to avoid exceptions
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content preview: {content.Substring(0, Math.Min(100, content.Length))}");
            }
            else
            {
                Console.WriteLine($"Response error: {response.ReasonPhrase}");
            }

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task SearchAPI_WithValidRequest_ReturnsCorrectResults()
        {
            // Act - Make request
            var response = await _httpClient.GetAsync("api/partysearch/search?searchTerm=Acme");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(
                content, _jsonOptions);

            searchResponse.Should().NotBeNull();
            searchResponse.Data.Should().HaveCountGreaterThan(0);
            searchResponse.Data.Should().Contain(p => p.Name.Contains("Acme"));
        }

        [Test]
        public async Task SearchAPI_WithFilters_ReturnsFilteredResults()
        {
            // Act - Use query parameters
            var response = await _httpClient.GetAsync(
                "api/partysearch/search?searchTerm=Smith&type=Individual&sanctionsStatus=Approved");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(
                content, _jsonOptions);

            searchResponse.Should().NotBeNull();
            searchResponse.Data.Should().OnlyContain(p =>
                p.Name.Contains("Smith") &&
                p.Type == PartyType.Individual &&
                p.SanctionsStatus == SanctionsStatus.Approved);
        }

        [Test]
        public async Task SearchAPI_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange - override with pagination-specific data
            await _repository.ClearAllAsync();
            await SeedLargeDataset(30, "Test");

            // Act - Request page 2 with 10 items per page
            var response = await _httpClient.GetAsync(
                "api/partysearch/search?searchTerm=Test&page=2&pageSize=10");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(
                content, _jsonOptions);

            searchResponse.Should().NotBeNull();
            searchResponse.Data.Should().HaveCount(10); // Second page should have 10 items
            searchResponse.Pagination.CurrentPage.Should().Be(2);
            searchResponse.Pagination.PageSize.Should().Be(10);
            searchResponse.Pagination.TotalResults.Should().Be(30);
            searchResponse.Pagination.TotalPages.Should().Be(3);
        }

        [Test]
        public async Task SearchAPI_WithNoResults_ReturnsEmptyList()
        {
            // Act - Search for something that doesn't exist
            var response = await _httpClient.GetAsync(
                "api/partysearch/search?searchTerm=Nonexistent");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(
                content, _jsonOptions);

            searchResponse.Should().NotBeNull();
            searchResponse.Data.Should().BeEmpty();
            searchResponse.Pagination.TotalResults.Should().Be(0);
        }

        [Test]
        public async Task SearchAPI_CaseInsensitive_ReturnsCaseInsensitiveMatches()
        {
            // Act - Search with different case
            var response = await _httpClient.GetAsync(
                "api/partysearch/search?searchTerm=acme");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(
                content, _jsonOptions);

            searchResponse.Should().NotBeNull();
            searchResponse.Data.Should().Contain(p => p.Name == "Acme Corporation");
        }

        private async Task SeedTestData()
        {
            var parties = new List<Party>
            {
                new Party {
                    PartyId = "P12345678",
                    Name = "Acme Corporation",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "95%"
                },
                new Party {
                    PartyId = "P87654321",
                    Name = "John Smith",
                    Type = PartyType.Individual,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "90%"
                },
                new Party {
                    PartyId = "P87654322",
                    Name = "Jane Smith",
                    Type = PartyType.Individual,
                    SanctionsStatus = SanctionsStatus.PendingReview,
                    MatchScore = "85%"
                },
                new Party {
                    PartyId = "P87654323",
                    Name = "Smith Organization",
                    Type = PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "80%"
                }
            };

            await _repository.AddPartiesAsync(parties);
        }

        private async Task SeedLargeDataset(int count, string namePrefix)
        {
            var parties = new List<Party>();

            for (int i = 1; i <= count; i++)
            {
                parties.Add(new Party
                {
                    PartyId = $"P{i:00000000}",
                    Name = $"{namePrefix} Party {i}",
                    Type = i % 2 == 0 ? PartyType.Individual : PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "80%"
                });
            }

            await _repository.AddPartiesAsync(parties);
        }
    }
}