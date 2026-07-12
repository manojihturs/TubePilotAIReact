using Microsoft.AspNetCore.DataProtection;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services
{
    public class ApiKeyEncryptionService : IApiKeyEncryptionService
    {
        private readonly IDataProtector _protector;

        public ApiKeyEncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("TubePilot.UserApiKeys.v1");
        }

        public string Encrypt(string plainText) => _protector.Protect(plainText);

        public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
    }
}
