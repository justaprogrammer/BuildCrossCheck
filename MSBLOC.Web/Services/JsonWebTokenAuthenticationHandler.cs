using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MSBLOC.Web.Services
{
    public class JsonWebTokenAuthenticationHandler : AuthenticationHandler<JsonWebTokenAuthenticationOptions>
    {
        public JsonWebTokenAuthenticationHandler(IOptionsMonitor<JsonWebTokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                //Authorization header not in request
                return AuthenticateResult.NoResult();
            }

            return AuthenticateResult.NoResult();
        }
    }

    public class JsonWebTokenAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}
