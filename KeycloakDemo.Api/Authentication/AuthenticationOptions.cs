namespace KeycloakDemo.Api.Authentication;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string MetadataUrl { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; }
    public string RsaPublicKey { get; set; } = string.Empty;
}
