using GTRevo.DataAccess.Entities;
using System;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Security
{
    public class TokenValidator : ITokenValidator
    {
        private ICrudRepository repository;

        public TokenValidator(ICrudRepository repository)
        {
            this.repository = repository;
        }

        public Guid GenerateToken(string serviceName)
        {
            var token = new ServiceToken(Guid.NewGuid(), serviceName);
            repository.Add(token);
			repository.SaveChanges();
			return token.Id;
        }

        public Task<Guid> GenerateTokenAsync(string serviceName)
        {
            return Task.Factory.StartNew(() => 
                {
                    return GenerateToken(serviceName);
                }
            );
        }

        public bool Validate(Guid token, string serviceName)
        {
            var tkn = repository.FirstOrDefaultAsync<ServiceToken>(t => t.Id == token && t.ServiceName == serviceName).Result;
            if (tkn != null)
            {
                repository.Remove(tkn);
                repository.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<bool> ValidateAsync(Guid token, string serviceName)
        {
            var tkn = await repository.FirstOrDefaultAsync<ServiceToken>(t => t.Id == token && t.ServiceName == serviceName);
            if (tkn != null)
            {
                repository.Remove(tkn);
                await repository.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
