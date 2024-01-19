# Title

## Configure Keycloak
### Docker Compose

### Admin UI
#### Create a new realm if needed. Do not use the master realm.

#### Create new clients
- Client for API: e.g. `keycloak-demo-api`
- Client for frontend: e.g. `keycloak-demo-frontend`

#### Create one new user (e.g. user1@test.com)
- Mark email as validated
- Apply a password; turn off the **Temporary** switch

## Configure API
### Nuget
Add following Nuget package: `Microsoft.AspnetCore.Authentication.JwtBearer`

### Appsettings
```json
{
    ...
    "Authentication": {
        "Audience": "<keycloak-client-id>",
        "ValidIssuer": "http://localhost:8080/realms/keycloak-demo",
        "MetadataUrl": "http://localhost:8080/realms/keycloak-demo/.well-known/openid-configuration",
        "RequireHttpsMetadata": false | true
    }
}
```

### Supporting classes
- Configure JWT authentication options (inside `Authentication` folder)
```CSharp
public sealed class AuthenticationOptions
{
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string MetadataUrl { get; set; } = string.Empty;
    public bool RequireHttsMetadata { get; set; }
}
public sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>
{
    private readonly AuthenticationOptions _authenticationOptions;
    public JwtBearerOptionsSetup(IOptions<AuthenticationOptions> authenticationOptiosn)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _authenticationOptions.Audience;
        options.MetadataAddress = _authenticationOptions.MetadataUrl;
        options.RequireHttpsMetadat = _authenticationOptions.RequireHttpsMetadata;
        options.TokenValidationParameters.ValidIssuer = _authenticationOptions.Issuer;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
```

### Program.cs
- Configure Services
```CSharp
var config = builder.Services.Configuration;
builder.Services.Configure<AuthenticationOptions>(config.GetSection("Authentication"));
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {

    });
```
- Configure middleware: add the following as early as possible in pipeline (but not too early)
```CSharp
app.UseAuthentication();
app.UseAuthorization();
```

## Configure the Front-end SPA