using AnarchyChess.Api.Auth.Errors;
using ErrorOr;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Auth.Services;

public interface IOAuthProviderNameNormalizer
{
    ErrorOr<string> NormalizeProviderName(string providerName);
}

public class OAuthProviderNameNormalizer : IOAuthProviderNameNormalizer
{
    private readonly Dictionary<string, string> ProviderNameMap = new()
    {
        { "google", Providers.Google },
        { "discord", Providers.Discord },
    };

    public ErrorOr<string> NormalizeProviderName(string providerName) =>
        ProviderNameMap.TryGetValue(providerName.ToLower(), out var normalizedName)
            ? normalizedName
            : AuthErrors.OAuthProviderNotFound;
}
