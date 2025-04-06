﻿using Microsoft.AspNetCore.Mvc;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Services;

namespace PartySearchApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartySearchController(IPartySearchService service) : ControllerBase
    {
        private readonly IPartySearchService _service = service;

        [HttpGet("search")]
        public async Task<ActionResult<SearchResponse>> Search(
            [FromQuery] string searchTerm,
            [FromQuery] string? type = null,
            [FromQuery] string? sanctionsStatus = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            PartyType? parsedType = (type == null) ? null : ParseEnum<PartyType>(type);
            SanctionsStatus? parsedStatus = (sanctionsStatus == null) ? null : ParseEnum<SanctionsStatus>(sanctionsStatus);

            var request = new SearchRequest
            {
                SearchTerm = searchTerm,
                Type = parsedType,
                SanctionsStatus = parsedStatus,
                Page = page,
                PageSize = pageSize
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