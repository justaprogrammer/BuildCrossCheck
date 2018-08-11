using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using MSBLOC.Web.Interfaces;

namespace MSBLOC.Web.Services
{
    public class AccessTokenAuthenticationHandler : AuthenticationHandler<AccessTokenAuthenticationOptions>
    {
        public const string SchemeName = "MSBLOC.Api.Scheme";
        private readonly IAccessTokenService _accessTokenService;

        public AccessTokenAuthenticationHandler(IOptionsMonitor<AccessTokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAccessTokenService accessTokenService) : base(options, logger, encoder, clock)
        {
            _accessTokenService = accessTokenService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                //Authorization header not in request
                return AuthenticateResult.Fail("No Authorization Header Found.");
            }

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
            {
                //Authorization header invalid
                return AuthenticateResult.Fail("Invalid Authorization Header.");
            }

            try
            {
                var bearer = authorizationHeader.Substring("Bearer ".Length);

                var jwt = await _accessTokenService.ValidateTokenAsync(bearer);

                var identity = new ClaimsIdentity(jwt.Claims, SchemeName);

                var principal = new ClaimsPrincipal(identity);

                var ticket = new AuthenticationTicket(principal, SchemeName);

                return AuthenticateResult.Success(ticket);
            }
            catch(Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }
    }

    public class AccessTokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        
    }
}
