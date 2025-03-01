using ExpenseTracker.Common.Abstractions.OAuth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController(
        IOAuthService oauthService,
        IOAuthUserService oauthUserService,
        IOAuthProviderService providerService,
        IConfiguration configuration,
        ILogger<OAuthController> logger) : ControllerBase
    {
        private readonly IOAuthService _oauthService = oauthService;
        private readonly IOAuthUserService _oauthUserService = oauthUserService;
        private readonly IOAuthProviderService _providerService = providerService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<OAuthController> _logger = logger;

        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            return returnUrl is null ? throw new ArgumentNullException(nameof(returnUrl))
                : (IActionResult)Ok(new { loginUrl = Url.Action("ExternalLogin") });
        }

        [HttpGet("login/{provider}")]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl = null!)
        {
            try
            {
                returnUrl ??= _configuration["App:ClientUrl"]!;

                if (!_providerService.IsProviderSupported(provider))
                {
                    return BadRequest(new { message = $"Unsupported provider: {provider}" });
                }

                var properties = await _oauthService.ConfigureExternalAuthenticationAsync(provider, returnUrl);
                var normalizedProvider = _providerService.NormalizeProviderName(provider);

                return Challenge(properties, normalizedProvider);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Authentication challenge failed: " + ex.Message });
            }
        }

        [HttpGet("microsoft-callback")]
        public async Task<IActionResult> MicrosoftCallback()
        {
            return await HandleCallbackAsync("Microsoft");
        }

        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            return await HandleCallbackAsync("GitHub");
        }

        [HttpGet("status")]
        public async Task<IActionResult> CheckAuthStatus()
        {
            try
            {
                if (!IsUserAuthenticated())
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                _logger.LogInformation("Auth status check for authenticated user");

                var result = await _oauthUserService.GetCurrentUserStatusAsync(User);

                if (!result.IsSuccess)
                {
                    return Ok(new { success = false, error = result.ErrorMessage });
                }

                return Ok(new { success = true, user = result.User });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAuthStatus");
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("ensure-user")]
        public async Task<IActionResult> EnsureUserExists()
        {
            if (!IsUserAuthenticated())
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            _logger.LogInformation("EnsureUserExists called for authenticated user");

            var result = await _oauthUserService.EnsureUserExistsAsync(User);

            return Ok(new
            {
                success = result.IsSuccess,
                user = result.User,
                error = result.ErrorMessage
            });
        }

        [HttpGet("handle-state-failure")]
        public async Task<IActionResult> HandleStateFailure(string provider, string error)
        {
            _logger.LogInformation("Handling state validation failure for {Provider}", provider);

            try
            {
                // More thorough authentication check
                if (User.Identity?.IsAuthenticated == true &&
                    !string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                {
                    try
                    {
                        var result = await _oauthUserService.EnsureUserExistsAsync(User);
                        if (!result.IsSuccess)
                        {
                            _logger.LogWarning("Failed to ensure user exists: {Error}", result.ErrorMessage);
                            return Redirect($"{_configuration["App:ClientUrl"]}?error=user_creation_failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error ensuring user exists during state failure handling");
                        // return Redirect($"{_configuration["App:ClientUrl"]}?error=state_failure&message={Uri.EscapeDataString(ex.Message)}");
                    }

                    return Redirect($"{_configuration["App:ClientUrl"]}/dashboard?auth=success");
                }

                return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider={provider}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleStateFailure");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=unknown&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        private bool IsUserAuthenticated()
        {
            // Check multiple criteria to verify authentic authentication
            return User.Identity?.IsAuthenticated == true &&
                   !string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)) &&
                   (!string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.Email)) ||
                    !string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.Name)));
        }

        private async Task<IActionResult> HandleCallbackAsync(string provider)
        {
            try
            {
                if (!Request.Query.ContainsKey("code") && !Request.Query.ContainsKey("state"))
                {
                    var noParamsResult = await _oauthService.HandleCallbackWithoutParamsAsync(
                        User,
                        provider,
                        _configuration["App:ClientUrl"]!);

                    return Redirect(noParamsResult.RedirectUrl);
                }

                var loginResult = await _oauthService.ProcessCallbackAsync(
                    provider,
                    HttpContext,
                    _configuration["App:ClientUrl"]!);

                return Redirect(loginResult!.RedirectUrl);
            }
            catch (Exception ex)
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    return Redirect($"{_configuration["App:ClientUrl"]}/dashboard?auth=success&error_handled=true");
                }

                return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider={provider}&message={Uri.EscapeDataString(ex.Message)}");
            }
        }
    }
}