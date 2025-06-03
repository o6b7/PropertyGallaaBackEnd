using PropertyGalla.Models;

namespace PropertyGalla.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }//
}
