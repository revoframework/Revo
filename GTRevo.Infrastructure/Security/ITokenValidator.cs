using System;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Security
{
    public interface ITokenValidator
    {
        Guid GenerateToken(string serviceName);
        Task<Guid> GenerateTokenAsync(string serviceName);
        bool Validate(Guid token, string serviceName);
        Task<bool> ValidateAsync(Guid token, string serviceName);
    }
}
