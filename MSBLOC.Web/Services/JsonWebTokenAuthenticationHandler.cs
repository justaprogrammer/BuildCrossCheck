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
    public class JsonWebTokenAuthenticationHandler : AuthenticationHandler<JsonWebTokenAuthenticationOptions>
    {
        public const string SchemeName = "MSBLOC.Api.Scheme";
        private readonly IJsonWebTokenService _jsonWebTokenService;

        public JsonWebTokenAuthenticationHandler(IOptionsMonitor<JsonWebTokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IJsonWebTokenService jsonWebTokenService) : base(options, logger, encoder, clock)
        {
            _jsonWebTokenService = jsonWebTokenService;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
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

                var tokenValidationResult = _jsonWebTokenService.ValidateToken(bearer);

                var jwt = tokenValidationResult.SecurityToken as JsonWebToken;

                if(jwt == null) throw new Exception("Invalid token format.");

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

    public class JsonWebTokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        
    }
}
