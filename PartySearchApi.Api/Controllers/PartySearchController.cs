using Microsoft.AspNetCore.Mvc;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Services;

namespace PartySearchApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartySearchController(IPartySearchService service) : ControllerBase
    {
        private readonly IPartySearchService _service = service;

        /// <summary>
        /// Search for parties by name or ID with optional filtering
        /// </summary>
        /// <param name="searchTerm">Full or partial name or ID to search for</param>
        /// <param name="type">Filter by party type (Individual or Organization)</param>
        /// <param name="sanctionsStatus">Filter by sanctions status (Approved, PendingReview, Escalated, ConfirmedMatch, FalsePositive)</param>
        /// <param name="page">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <returns>List of matching parties with pagination metadata</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResponse>> Search(
            [FromQuery] string searchTerm = "",
            [FromQuery] string? type = null,
            [FromQuery] string? sanctionsStatus = null,
            [FromQuery] string page = "1",
            [FromQuery] string pageSize = "10")
        {
            PartyType? parsedType = (type == null) ? null : ParseEnum<PartyType>(type);
            SanctionsStatus? parsedStatus = (sanctionsStatus == null) ? null : ParseEnum<SanctionsStatus>(sanctionsStatus);

            int parsedPage = int.TryParse(page, out int pageValue) ? pageValue : 1;
            int parsedPageSize = int.TryParse(pageSize, out int pageSizeValue) ? pageSizeValue : 10;

            var request = new SearchRequest
            {
                SearchTerm = searchTerm,
                Type = parsedType,
                SanctionsStatus = parsedStatus,
                Page = parsedPage,
                PageSize = parsedPageSize
            };

            var response = await _service.SearchPartiesAsync(request);
            return Ok(response);
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