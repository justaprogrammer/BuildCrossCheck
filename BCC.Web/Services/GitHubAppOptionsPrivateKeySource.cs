using BCC.Web.Models;

namespace BCC.Web.Services
{
    public class GitHubAppOptionsPrivateKeySource : IPrivateKeySource
    {
        private readonly IOptions<GitHubAppOptions> _optionsAccessor;

        public GitHubAppOptionsPrivateKeySource(IOptions<GitHubAppOptions> optionsAccessor)
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
