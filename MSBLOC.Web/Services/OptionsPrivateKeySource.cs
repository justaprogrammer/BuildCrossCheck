using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHubJwt;
using Microsoft.Extensions.Options;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Services
{
    public class OptionsPrivateKeySource : IPrivateKeySource
    {
        private readonly IOptions<GitHubAppOptions> _optionsAccessor;

        public OptionsPrivateKeySource(IOptions<GitHubAppOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public TextReader GetPrivateKeyReader()
        {
            return new StringReader(CreatePem(_optionsAccessor.Value.PrivateKey));
        }

        private static string CreatePem(string input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
            stringBuilder.AppendLine(input);
            stringBuilder.AppendLine("-----END RSA PRIVATE KEY-----");
            return stringBuilder.ToString();
        }
    }
}
