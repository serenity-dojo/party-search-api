using Microsoft.AspNetCore.Mvc;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Services;

namespace PartySearchApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartiesController(IPartyService partyService) : ControllerBase
    {
        private readonly IPartyService _partyService = partyService;

        /// <summary>
        /// Search for parties by name or ID with optional filtering
        /// </summary>
        /// <param name="query">Full or partial name or ID to search for</param>
        /// <param name="type">Filter by party type (Individual or Organization)</param>
        /// <param name="sanctionsStatus">Filter by sanctions status (Approved, PendingReview, Escalated, ConfirmedMatch, FalsePositive)</param>
        /// <param name="page">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <returns>List of matching parties with pagination metadata</returns>
        [HttpGet]  // Maps GET requests to /api/parties
        [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResponse>> Search(
            [FromQuery] string query = "",
            [FromQuery] string? type = null,
            [FromQuery] string? sanctionsStatus = null,
            [FromQuery] string page = "1",
            [FromQuery] string pageSize = "10")
        {

        Console.WriteLine($"Query: {query}, Type: {type}, SanctionsStatus: {sanctionsStatus}, Page: {page}, PageSize: {pageSize}"); 

            PartyType? parsedType = (type == null) ? null : ParseEnum<PartyType>(type);
            SanctionsStatus? parsedStatus = (sanctionsStatus == null) ? null : ParseEnum<SanctionsStatus>(sanctionsStatus);

            int parsedPage = int.TryParse(page, out int pageValue) ? pageValue : 1;
            int parsedPageSize = int.TryParse(pageSize, out int pageSizeValue) ? pageSizeValue : 10;

            var request = new SearchRequest
            {
                SearchTerm = query,
                Type = parsedType,
                SanctionsStatus = parsedStatus,
                Page = parsedPage,
                PageSize = parsedPageSize
            };
        
            Console.WriteLine($"Parsed Request: {request}");

            var response = await _partyService.SearchPartiesAsync(request);

            Console.WriteLine($"Response: {response}");

            return Ok(response);
        }

        /// <summary>
        /// Add a new party to the repository
        /// </summary>
        /// <param name="party">The party to be added</param>
        /// <returns>Action result indicating success or failure</returns>
        [HttpPost] // Maps POST requests to /api/parties
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddParty([FromBody] Party party)
        {
            if (party == null)
            {
                return BadRequest("Party cannot be null.");
            }

            try
            {
                await _partyService.OnboardPartyAsync(party);
                return CreatedAtAction(nameof(Search), new { query = party.PartyId }, party);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static TEnum? ParseEnum<TEnum>(string value) where TEnum : struct
        {
            if (!string.IsNullOrEmpty(value) && Enum.TryParse<TEnum>(value, true, out var result))
            {
                return result;
            }
            return null;
        }

    }
}