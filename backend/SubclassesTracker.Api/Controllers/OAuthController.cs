using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SubclassesTracker.Api.Utils;
using SubclassesTracker.Models;
using SubclassesTracker.Models.Requests.Api;
using SubclassesTracker.Models.Responses;
using System.Collections.Concurrent;

namespace SubclassesTracker.Api.Controllers;

[ApiController]
[Route("api/oauth")]
public sealed class OAuthController(
    IHttpClientFactory httpFactory,
    IOptions<LinesConfig> cfg,
    ILogger<OAuthController> logger) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, string> _verifiers = new();

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly LinesConfig _cfg = cfg.Value;

    /// <summary>
    /// Returns OAuth Esologs Url
    /// </summary>
    /// <param name="authUriRequest">Requests to get the auth url</param>
    /// <returns>Auth url in esologs</returns>
    [AllowAnonymous]
    [HttpGet("url")]
    public IActionResult GetAuthUrl(
        [FromQuery] AuthUriApiRequests authUriRequest)
    {
        var codeVerifier = PKCEHelper.GenerateCodeVerifier();
        var codeChallenge = PKCEHelper.GenerateCodeChallenge(codeVerifier);

        _verifiers[authUriRequest.ClientId] = codeVerifier;

        var qs = new QueryBuilder {
            { "response_type", "code" },
            { "client_id", authUriRequest.ClientId },
            { "redirect_uri", authUriRequest.RedirectUrl },
            { "code_challenge", codeChallenge },
            { "code_challenge_method", "S256" },
            { "scope", "view-user-profile view-private-reports" }
        };

        var url = $"{_cfg.AuthEndpoint}{qs.ToQueryString()}";
        logger.LogInformation($"The client {authUriRequest.ClientId} start authentication");

        return Ok(new { url });
    }

    /// <summary>
    /// Get access token by the code
    /// </summary>
    /// <param name="dto">exchange request</param>
    /// <returns>New token</returns>
    [AllowAnonymous]
    [HttpPost("exchange")]
    public async Task<IActionResult> ExchangeCode(
        [FromBody] ExchangeApiRequest dto,
        CancellationToken cts = default)
    {
        if (!_verifiers.TryRemove(dto.ClientId, out var codeVerifier))
            return BadRequest("invalid_state");

        var http = _httpFactory.CreateClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>{
            { "grant_type", "authorization_code" },
            { "client_id", dto.ClientId },
            { "redirect_uri", dto.RedirectUri },
            { "code", dto.Code },
            { "code_verifier", codeVerifier }
        });

        var resp = await http.PostAsync(_cfg.TokenEndpoint, content, cts);
        var json = await resp.Content.ReadAsStringAsync(cts);

        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, json);

        var token = JsonConvert.DeserializeObject<TokenResponse>(json)
            ?? new TokenResponse();

        logger.LogInformation($"The client {dto.ClientId} authenticated");

        return Ok(token);
    }

    /// <summary>
    /// Get the new AccessToken by the RefreshToken
    /// </summary>
    /// <param name="dto">Request params</param>
    /// <returns>New tokens</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshApiRequest dto,
        CancellationToken cts = default)
    {
        var http = _httpFactory.CreateClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>{
            { "grant_type", "refresh_token" },
            { "client_id", dto.ClientId },
            { "refresh_token", dto.RefreshToken }
        });

        var resp = await http.PostAsync(_cfg.TokenEndpoint, content, cts);
        var json = await resp.Content.ReadAsStringAsync(cts);

        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, json);

        var token = JsonConvert.DeserializeObject<TokenResponse>(json)
            ?? new TokenResponse();

        return Ok(token);
    }

    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromBody] ExchangeApiRequest dto,
        CancellationToken cts = default)
    {
        if (!_verifiers.TryRemove(dto.ClientId, out var codeVerifier))
            return BadRequest("invalid_state");

        var http = _httpFactory.CreateClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", dto.ClientId },
                { "redirect_uri", dto.RedirectUri },
                { "code", dto.Code },
                { "code_verifier", codeVerifier }
            });

        var resp = await http.PostAsync(_cfg.TokenEndpoint, content, cts);
        var json = await resp.Content.ReadAsStringAsync(cts);

        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, json);

        var token = JsonConvert.DeserializeObject<TokenResponse>(json)
            ?? new TokenResponse();

        return Ok(token);
    }
}