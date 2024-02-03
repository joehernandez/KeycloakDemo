# Keycloak with ASP.NET Core Web API and SPA

## Keycloak Configuration
### Create a New Realm (if needed)
- Click on the dropdown that shows the current realm
- Click on **Create realm**
- Enter the **Realm name** and then click on **Create**
- Configure the realm as needed using the Keycloak [guide](https://www.keycloak.org/docs/latest/server_admin/index.html#configuring-realms)
    - At a minimum, configure: **Login** and **Email**

### Create Realm Roles
- Click on **Realm roles** in left sidebar
- Click on the **Create role** button
- Enter the **Role name** and click on the **Save** button
    - E.g., create the following realm roles
        - **admin**
        - **user**
- To assign a default role during user registration
    - Click on **Realm Roles** in left sidebar
    - Click on the **default-roles-{realm-name}** link (e.g. [default-roles-keyclock-demo]())
    - Click on the **Assign role** button
    - Check the role that should be assigned during user registration (e.g. **user**)
    - Click on the **Assign** button
- Additional setup to make roles work out-of-the-box with ASP.NET Core `JwtBearer`
    - Click on **Client scopes** in left sidebar
    - Click on **roles** link (under **Name**)
    - Click on the **Mappers** tab => **Add mapper** button
        - Select **By configuration**
        - Select **User Realm Role**
    - Fill in as follows:
        - Name: Something descriptive (e.g. **Realm roles for ASP.NET Core JwtBearer**)
        - Realm Role prefix: leave blank
        - Multivalued: **On**
        - Token Claim Name: **roles** 
        - Leave the rest with default values
    - Click on **Save**
    - Used [this](https://stackoverflow.com/questions/56327794/role-based-authorization-using-keycloak-and-net-core) Stack Overflow question/answer for reference 
        - Use the 2nd alternative in the accepted answer
        - This leaves the original `"realm_roles": { "roles": [...]}` top-level object and adds the new `"roles": [...]` top-level object needed by `JwtBearer`

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
    - **Web origins**: `http://localhost:5000`, `https://oauth.pstmn.io`
        - **Note** forward slash + asterisk (`/*`) at end of first one
            - If **Valid redirect URIs** is missing `/*` you get `Invalid redirect_uri paramater` errors
        - **Note** no trailing forward slash (`/`) on second one
            - If **Web Origins** has a trailing slash, you get CORS errors
        - **Instead of specifying URLs, use `+` to allow all origins specified in valid redirect URIs** (*this is the method I used*)

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
### The blog post that helped me the most in putting this together was [this one](https://blog.devgenius.io/security-in-react-and-webapi-in-asp-net-core-c-with-authentification-and-authorization-by-keycloak-f890d340d093)
Without using the public key as described in the blog post, I could not get the Web API JWT security middleware to validate the Keycloak token

### Nuget package
Add `Microsoft.AspnetCore.Authentication.JwtBearer`

### Appsettings
- Add following to `appsettings.json`  
    - `RsaPublicKey` is in user secrets **not** because it needs to be kept a secret but because it will be different on each development machine and different environments (e.g. **Test**, **Prod**)
```json
{
    ...
    "Authentication": {
        "MetadataUrl": "http://localhost:8890/realms/strainth/.well-known/openid-configuration",
        "Issuer": "http://localhost:8890/realms/strainth",
        "RequireHttpsMetadata": false,
        "RsaPublicKey": "IN_USER_SECRETS"
    }
}
```

### AuthenticationOptions and AddAuthExtensions
- `AuthenticationOptions.cs`
```csharp
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string MetadataUrl { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; }
    public string RsaPublicKey { get; set; } = string.Empty;
}
```
- `AddAuthExtensions`
    - The `JwtBearerEvents` are useful when setting up initially for debugging purposes, but are not necessary for functionality and may be removed
```csharp
public static class AddAuthExtensions
{
    public static IServiceCollection AddAuthenticationWithJwt(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });
        var authOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()!;
        authenticationBuilder.AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
            options.MetadataAddress = authOptions.MetadataUrl;

            #region == JWT Token Validation ==
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = authOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = BuildRsaKey(authOptions.RsaPublicKey),
                ValidateLifetime = true,
            };
            #endregion
            #region == Event Authentication Handlers ==
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = c =>
                {
                    Console.WriteLine("User successfully authenticated");
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = c =>
                {
                    c.NoResult();
                    c.Response.ContentType = "text/plain";
                    if (isDevelopment)
                    {
                        return c.Response.WriteAsync(c.Exception.ToString());
                    }
                    return c.Response.WriteAsync("An error occured processing your authentication.");
                }
            };
            #endregion
        });
        return services;
    }
    private static RsaSecurityKey BuildRsaKey(string publicKey)
    {
        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(
            source: Convert.FromBase64String(publicKey),
            bytesRead: out _);
        var issuerSigningKey = new RsaSecurityKey(rsa);
        return issuerSigningKey;
    }
}
```

### JWT Configuration in `Program.cs`
- Make use of `AuthenticationOptions` and `AddAuthExtensions`
    - Configure Authentication scheme to be JWT Bearer
    - add the `UseAuthentication` and `UseAuthorization` calls to the middleware configuration
```csharp
...
var config = builder.Configuration;
builder.Services.Configure<AuthenticationOptions>(config.GetSection(AuthenticationOptions.SectionName));
builder.Services.AddAuthenticationWithJwt(config, builder.Environment.IsDevelopment());
...
app.UseAuthentication();
app.UseAuthorization();
```

### CORS configuration
[CORS is not a security feature. CORS is a W3C standard that allows a server to relax the same-origin policy](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0#how-cors-works)  
CORS only affects browsers, which are the ones that block requests when CORS is not configured on the API server, or it is misconfigured
#### Appsettings
```json
{
    ...
    "CorsAllowedOrigins": [
        "http://localhost:5000",
        "http://localhost:5001"
    ]
}
```
#### `Program.cs`
```csharp
var config = builder.Configuration;
builder.Services.Configure<AuthenticationOptions>(config.GetSection(AuthenticationOptions.SectionName));
builder.Services.AddAuthenticationWithJwt(config, isDevelopment);
...
var allowedOrigins = config.GetSection("CorsAllowedOrigins").Get<List<string>>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
            .WithOrigins(allowedOrigins?.ToArray() ?? [""])
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
...
app.UseAuthentication();
app.UseCors(); // <== ADDED!
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
    - May need to first click on the **Clear cookies** button
- Used [this help article](https://learning.postman.com/docs/sending-requests/authorization/oauth-20/)
