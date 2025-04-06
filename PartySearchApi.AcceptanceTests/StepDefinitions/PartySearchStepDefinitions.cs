using System.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PartySearchApi.AcceptanceTests.Model;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using Reqnroll;

namespace PartySearchApi.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class PartySearchStepDefinitions
    {
        private readonly HttpClient _client;
        private readonly InMemoryPartyRepository _repository = new();
        private SearchRequest? _searchRequest;
        private SearchResponse? _searchResponse;

        public PartySearchStepDefinitions()
        {
            // Create WebApplicationFactory with our test repository
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    _ = builder.ConfigureServices(services =>
                    {
                        // Remove existing repository registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IPartyRepository));

                        if (descriptor != null)
                        {
                            _ = services.Remove(descriptor);
                        }

                        // Add our test repository instance
                        _ = services.AddSingleton<IPartyRepository>(_repository);
                    });
                });

            _client = factory.CreateClient();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            // Clear any data from previous scenarios
            await _repository.ClearAllAsync();
        }

        [Given(@"the following parties exist:")]
        public async Task GivenTheFollowingPartiesExist(Table table)
        {
            var parties = table.CreateSet<PartyDto>().Select(MapToParty).ToList();

            // Directly add to repository
            await _repository.AddPartiesAsync(parties);
        }

        [Given(@"{int} parties exist with a name containing {string}")]
        public async Task GivenPartiesExistWithANameContaining(int count, string nameContains)
        {
            var parties = new List<Party>();

            for (int i = 1; i <= count; i++)
            {
                parties.Add(new Party
                {
                    PartyId = $"P{i:00000000}",
                    Name = $"{nameContains} Test {i}",
                    Type = i % 2 == 0 ? PartyType.Individual : PartyType.Organization,
                    SanctionsStatus = SanctionsStatus.Approved,
                    MatchScore = "80%"
                });
            }

            // Directly add to repository
            await _repository.AddPartiesAsync(parties);
        }

        [When(@"{word} searches for {string}")]
        public async Task WhenSearchesFor(string _, string searchTerm)
        {
            _searchRequest = new SearchRequest(searchTerm);
            await PerformSearch();
        }

        [When(@"{word} searches for {string} with the following filters:")]
        public async Task WhenSearchesForWithTheFollowingFilters(string _, string searchTerm, Table table)
        {
            PartyType? type = null;
            SanctionsStatus? sanctionsStatus = null;
            foreach (var row in table.Rows)
            {
                if (row["Filter"] == "Type")
                {
                    type = Enum.Parse<PartyType>(row["Value"]);
                }
                else if (row["Filter"] == "Status" || row["Filter"] == "Sanction Status" || row["Filter"] == "Sanctions Status")
                {
                    sanctionsStatus = ParseSanctionsStatus(row["Value"]);
                }
            }
            _searchRequest = new SearchRequest(searchTerm, type, sanctionsStatus);

            await PerformSearch();
        }

        [When(@"{word} searches for {string} with the following parameters:")]
        public async Task WhenSearchesForWithTheFollowingParameters(string _, string searchTerm, Table table)
        {

            int page = 1;
            int pageSize = 10;

            foreach (var row in table.Rows)
            {
                if (row.ContainsKey("Page"))
                {
                    page = int.Parse(row["Page"]);
                }
                if (row.ContainsKey("pageSize"))
                {
                    pageSize = int.Parse(row["pageSize"]);
                }
            }
            _searchRequest = new SearchRequest(searchTerm, null, null, page, pageSize);


            await PerformSearch();
        }

        [Then(@"the search results should contain exactly:")]
        public void ThenTheSearchResultsShouldContainExactly(Table table)
        {
            _searchResponse.Should().NotBeNull();

            var expectedParties = table.CreateSet<PartyDto>().Select(MapToParty).ToList();

            _ = _searchResponse.Results.Should().HaveCount(expectedParties.Count);

            foreach (var expectedParty in expectedParties)
            {
                _ = _searchResponse.Results.Should().Contain(p =>
                    p.PartyId == expectedParty.PartyId &&
                    p.Name == expectedParty.Name &&
                    p.Type == expectedParty.Type &&
                    p.SanctionsStatus == expectedParty.SanctionsStatus
                );
            }
        }

        [Then(@"the search results should be empty")]
        public void ThenTheSearchResultsShouldBeEmpty()
        {
            _searchResponse.Should().NotBeNull();
            _searchResponse.Results.Should().BeEmpty();
        }

        [Then(@"the parties returned should be items {int}-{int} of the complete result set")]
        public void ThenThePartiesReturnedShouldBeItemsOfTheCompleteResultSet(int start, int end)
        {
            _searchResponse.Should().NotBeNull();
            _searchResponse.Results.Should().HaveCount(end - start + 1);
        }

        [Then(@"the response should include pagination metadata:")]
        public void ThenTheResponseShouldIncludePaginationMetadata(Table table)
        {
            _searchResponse.Should().NotBeNull();
            foreach (var row in table.Rows)
            {
                if (row.ContainsKey("totalResults"))
                {
                    _ = _searchResponse.Pagination.TotalResults.Should().Be(int.Parse(row["totalResults"]));
                }
                if (row.ContainsKey("totalPages"))
                {
                    _ = _searchResponse.Pagination.TotalPages.Should().Be(int.Parse(row["totalPages"]));
                }
                if (row.ContainsKey("currentPage"))
                {
                    _ = _searchResponse.Pagination.CurrentPage.Should().Be(int.Parse(row["currentPage"]));
                }
                if (row.ContainsKey("pageSize"))
                {
                    _ = _searchResponse.Pagination.PageSize.Should().Be(int.Parse(row["pageSize"]));
                }
            }
        }

        private async Task PerformSearch()
        {
            _searchRequest.Should().NotBeNull();

            // Build query parameters
            var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
            queryBuilder["query"] = _searchRequest.SearchTerm;

            if (_searchRequest.Type.HasValue)
            {
                queryBuilder["type"] = _searchRequest.Type.Value.ToString();
            }

            if (_searchRequest.SanctionsStatus.HasValue)
            {
                queryBuilder["sanctionsStatus"] = _searchRequest.SanctionsStatus.Value.ToString();
            }

            queryBuilder["page"] = _searchRequest.Page.ToString();
            queryBuilder["pageSize"] = _searchRequest.PageSize.ToString();

            var queryString = queryBuilder.ToString();
            var response = await _client.GetAsync($"api/parties?{queryString}");
            _ = response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _searchResponse = JsonConvert.DeserializeObject<SearchResponse>(content);
        }

        private static SanctionsStatus ParseSanctionsStatus(string status)
        {
            return status switch
            {
                "Pending Review" => SanctionsStatus.PendingReview,
                "Confirmed Match" => SanctionsStatus.ConfirmedMatch,
                "False Positive" => SanctionsStatus.FalsePositive,
                _ => Enum.Parse<SanctionsStatus>(status)
            };
        }

        private Party MapToParty(PartyDto dto)
        {
            return new Party
            {
                PartyId = dto.PartyId,
                Name = dto.Name,
                Type = Enum.Parse<PartyType>(dto.Type),
                SanctionsStatus = ParseSanctionsStatus(dto.SanctionsStatus),
                MatchScore = dto.MatchScore
            };
        }
    }
}