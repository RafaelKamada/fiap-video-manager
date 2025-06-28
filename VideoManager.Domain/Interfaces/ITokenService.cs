namespace VideoManager.Domain.Interfaces
{
    public interface ITokenService
    {
        string GerarToken(string email);
    }
}
