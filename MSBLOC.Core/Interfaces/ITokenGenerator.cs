namespace MSBLOC.Core.Interfaces
{
    public interface ITokenGenerator
    {
        string GetToken(int expirationSeconds = 600);
    }
}