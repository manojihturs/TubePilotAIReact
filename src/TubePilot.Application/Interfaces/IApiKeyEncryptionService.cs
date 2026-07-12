namespace TubePilot.Application.Interfaces
{
    public interface IApiKeyEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
