namespace BCC.Core.Tests.Util
{
    public class TestPrivateKeySource : IPrivateKeySource
    {
        public TextReader GetPrivateKeyReader()
        {
            return new StreamReader(new MemoryStream(Resources.private_key));
        }
    }
}