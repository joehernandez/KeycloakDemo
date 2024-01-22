# Keycloak with ASP.NET Core Web API and SPA

## Keycloak Configuration
### Create a New Realm (if needed)
- Click on the dropdown that shows the current realm
- Click on **Create realm**
- Enter the **Realm name** and then click on **Create**

### Create Realm Roles
- Click on **Realm roles** in left sidebar
- Click on the **Create role** button
- Enter the **Role name** and click on the **Save** button
- E.g., create the following realm roles
    - **admin**
    - **user**

### Client Configuration: Front-End SPA
- Click on **Clients** >> **Create client**
- General Settings
    - Enter **Client ID** (e.g. keycloak-demo-frontend); 
    - Optionally, enter a **Name**
    - Click on **Next**
- Capability Config
    - **Client authentication**: off
    - **Authorization**: off
    - **Authentication flow**: check **Standard Flow** only
    - Click on **Next**
- Login settings: only following needed
    - **Valid redirect URIs**: 
        - `http://localhost:5000/*`: for SPA
        - `https://oauth.pstmn.io/v1/browser-callback/*`: for Postman
    - **Web origins**: http://localhost:5000
        - **Note** forward slash + asterisk (`/*`) at end of first one
            - If **Valid redirect URIs** is missing `/*` you get `Invalid redirect_uri paramater` errors
        - **Note** no trailing forward slash (`/`) on second one
            - If **Web Origins** has a trailing slash, you get CORS errors
        - Instead of specifying URLs, use `+` to allow all origins specified in valid redirect URIs
        
### Client Configuration: Web API (Client Credentials in OAuth2)
- Click on **Clients** >> **Create client**
- General Settings
    - Enter **Client ID** (e.g. keycloak-demo-backend); 
    - Optionally, enter a **Name**
    - Click on **Next**
- Capability Config
    - **Client authentication**: on
    - **Authorization**: off
    - **Authentication flow**: check **Service accounts roles** only
    - Click on **Next**
    - Click on **Save**

### Create at least one test user per realm role in Keycloak
- Role: **user** (under **Role mapping**)
    - Username: test1@test.com
    - Password: test1
- Role: **admin**
    - Username: admin1@test.com
    - Password: admin1


## SPA Configuration
### Use Keycloak JS in SPA
- `npm i keycloak-js`
- Create a keycloak config file in the `public` folder
    - `keycloak.json`
    ```json
    {
        "realm": "keycloak-demo",
        "auth-server-url": "http://localhost:8080/",
        "ssl-required": "external",
        "resource": "keycloak-demo-frontend",
        "public-client": true,
        "confidential-port": 0
    }
    ```
- Create a service that can be re-used in the application
    - `UserService.ts`
    ```js
    import Keycloak from "keycloak-js";
    const keycloak = new Keycloak('/keycloak.json');
    const initKeycloak = (onAuthenticationCallback:() => void) => {
        keycloak.init({
            onLoad: 'check-sso',
            silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
            pkceMethod: 'S256'
        })
        .then(() => {
            onAuthenticationCallback();
        })
    };
    const UserService = {
        initKeycloak,
        keycloak,
    };
    export default UserService;
    ```
- Initialize Keycloak in `main.tsx`
```js
const renderApp = () => ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
UserService.initKeycloak(renderApp);
```
- In components where needed, import `UserService` then make use of available functions and properties on the `keycloak` object
```js
import UserService from "../services/UserService"
function UnauthenticatedUser() {
  const kc = UserService.keycloak;
    return (
        <>
          <div>
            <h1>
                Hello Anonymous, please log in &nbsp;
                <button onClick={() => kc.login()}>Login</button>
            </h1>
          </div>
        </>
      )
}
export default UnauthenticatedUser
```
- To make calls to the secured API, use the `axios` http client 
    - Configure the axios request interceptor to add the `Authorization` header to each request
- Keycloak-js docs: https://www.keycloak.org/docs/latest/securing_apps/#_javascript_adapter

## ASP.NET Core Web API Configuration
### Nuget package
Add `Microsoft.AspnetCore.Authentication.JwtBearer`

### Appsettings
- Add following to `appsettings.json`  
    - For `Audience` value below, get token returned to SPA client after logging in and inspect it using jwt.io
```json
{
    ...
    "Authentication": {
        "Audience": "account",
        "ValidIssuer": "http://localhost:8080/realms/keycloak-demo",
        "MetadataUrl": "http://localhost:8080/realms/keycloak-demo/.well-known/openid-configuration",
        "RequireHttpsMetadata": false
    }
}
```

### AuthenticationOptions and JwtBearerOptionsSetup
- `AuthenticationOptions.cs`
```csharp
public sealed class AuthenticationOptions
{
    public string Audience { get; set; } = string.Empty;
    public string MetadataUrl { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; }
    public string Issuer { get; set; } = string.Empty;
}
```
- `JwtBearerOptionsSetup`
```csharp
public sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _authenticationOptions;
    public JwtBearerOptionsSetup(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }
    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _authenticationOptions.Audience;
        options.MetadataAddress = _authenticationOptions.MetadataUrl;
        options.RequireHttpsMetadata = _authenticationOptions.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = _authenticationOptions.Issuer
        };
    }
    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
```

### JWT Configuration in `Program.cs`
- Make use of `AuthenticationOptions` and `JwtBearerOptionsSetup`
    - Configure Authentication scheme to be JWT Bearer
    - add the `UseAuthentication` and `UseAuthorization` calls to the middleware configuration
```csharp
...
var config = builder.Configuration;
builder.Services.Configure<AuthenticationOptions>(config.GetSection("Authentication"));
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

...
app.UseAuthentication();
app.UseAuthorization();
```

### CORS configuration in `Program.cs`
```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

const string AllowSpecificOrigins = "_allowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins,
                      policy =>
                      {
                          policy
                            .WithOrigins(
                                "http://localhost:5000",
                                "http://localhost:5001")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                      });
});
...
app.UseAuthentication();
app.UseCors(AllowSpecificOrigins); // <== ADDED!
app.UseAuthorization();
```

### Postman
- Start a new collection in Postman
- In the **Authorization** tab, select **OAuth 2.0** from the dropdown
- In the **Configure New Token** section, enter the following values:
    - Name: Give the token a name, e.g. Keycloak Demo Token
    - Grant Type: Authorization Code
    - Callback URL: https://oauth.pstmn.io/v1/browser-callback
        - Be sure this was added as a **Valid Redirect URI** during the SPA frontend Keycloak setup
    - Auth URL: http://localhost:8080/realms/keycloak-demo/protocol/openid-connect/auth
    - Access Token URL: http://localhost:8080/realms/keycloak-demo/protocol/openid-connect/token
        - Both **Auth URL** and **Access Token URL** can be obtained by using the Keycloak Realm well-known endpoints
        - In Keycloak admin UI: **Realm Settings** => **General** => **Endpoints** => click on **OpenID Endpoint Configuration** link
    - Client ID: keycloak-demo-frontend
        - Use the SPA front-end client created in the Keycloak setup
    - Scope: use space-delimited list; by default, includes **profile** and **email**
- Click on **Get New Access Token**, then on **Use Token** after successful login
- To use a different user, click on the **Get New Access Token** button once again and then on **Use Token** after successful login
- Used [this help article](https://learning.postman.com/docs/sending-requests/authorization/oauth-20/)
