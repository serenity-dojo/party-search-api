using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.AcceptanceTests.Models;
using System.Web;
using Reqnroll;

namespace PartySearchApi.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class PartySearchStepDefinitions
    {
        private readonly HttpClient _client;
        private readonly InMemoryPartyRepository _repository = new();
        private SearchRequest _searchRequest;
        private SearchResponse _searchResponse;

        public PartySearchStepDefinitions()
        {
            // Create WebApplicationFactory with our test repository
            var factory = new WebApplicationFactory<Program>()
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

            _client = factory.CreateClient();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            // Clear any data from previous scenarios
            await _repository.ClearAllAsync();

            // Reset search request for each scenario
            _searchRequest = new SearchRequest();
        }

        [Given(@"the following parties exist:")]
        public async Task GivenTheFollowingPartiesExist(Table table)
        {
            var parties = table.CreateSet<PartyDto>().Select(MapToParty).ToList();

            // Directly add to repository
            await _repository.AddPartiesAsync(parties);
        }

        [Given(@"(.*) parties exist with a name containing ""(.*)""")]
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

        [Given(@"the Party API returns the following parties for the search query ""(.*)""")]
        public async Task GivenThePartyAPIReturnsTheFollowingParties(string searchQuery, Table table)
        {
            var parties = table.CreateSet<PartyDto>().Select(MapToParty).ToList();

            // Add to repository
            await _repository.AddPartiesAsync(parties);
        }

        [When(@"(.*) searches for ""(.*)""")]
        public async Task WhenSearchesFor(string person, string searchTerm)
        {
            _searchRequest = new SearchRequest(searchTerm);
            await PerformSearch();
        }

        [When(@"(.*) searches for ""(.*)"" and filters by Type ""(.*)""")]
        public async Task WhenSearchesForAndFiltersByType(string person, string searchTerm, string type)
        {
            _searchRequest = new SearchRequest(searchTerm, Enum.Parse<PartyType>(type));
            await PerformSearch();
        }

        [When(@"(.*) searches for ""(.*)"" with the following filters:")]
        public async Task WhenSearchesForWithTheFollowingFilters(string person, string searchTerm, Table table)
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

        [When(@"(.*) searches for ""(.*)"" with the following parameters:")]
        public async Task WhenSearchesForWithTheFollowingParameters(string person, string searchTerm, Table table)
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
            var expectedParties = table.CreateSet<PartyDto>().Select(MapToParty).ToList();

            _searchResponse.Data.Should().HaveCount(expectedParties.Count);

            foreach (var expectedParty in expectedParties)
            {
                _searchResponse.Data.Should().Contain(p =>
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
            _searchResponse.Data.Should().BeEmpty();
        }

        [Then(@"the parties returned should be items (.*)\-(.*) of the complete result set")]
        public void ThenThePartiesReturnedShouldBeItemsOfTheCompleteResultSet(int start, int end)
        {
            _searchResponse.Data.Should().HaveCount(end - start + 1);
        }

        [Then(@"the response should include pagination metadata:")]
        public void ThenTheResponseShouldIncludePaginationMetadata(Table table)
        {
            foreach (var row in table.Rows)
            {
                if (row.ContainsKey("totalResults"))
                {
                    _searchResponse.Pagination.TotalResults.Should().Be(int.Parse(row["totalResults"]));
                }
                if (row.ContainsKey("totalPages"))
                {
                    _searchResponse.Pagination.TotalPages.Should().Be(int.Parse(row["totalPages"]));
                }
                if (row.ContainsKey("currentPage"))
                {
                    _searchResponse.Pagination.CurrentPage.Should().Be(int.Parse(row["currentPage"]));
                }
                if (row.ContainsKey("pageSize"))
                {
                    _searchResponse.Pagination.PageSize.Should().Be(int.Parse(row["pageSize"]));
                }
            }
        }

        private async Task PerformSearch()
        {
            // Build query parameters
            var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
            queryBuilder["searchTerm"] = _searchRequest.SearchTerm;

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
            var response = await _client.GetAsync($"api/partysearch/search?{queryString}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _searchResponse = JsonConvert.DeserializeObject<SearchResponse>(content);
        }

        private SanctionsStatus ParseSanctionsStatus(string status)
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