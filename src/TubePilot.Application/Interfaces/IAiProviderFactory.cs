namespace TubePilot.Application.Interfaces
{
    public interface IAiProviderFactory
    {
        IAiProvider GetProvider(string providerName);
    }
}
