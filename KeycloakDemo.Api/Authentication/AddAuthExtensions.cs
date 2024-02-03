using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace KeycloakDemo.Api.Authentication;

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
            // uncomment for debugging
            //options.Events = new JwtBearerEvents
            //{
            //    OnTokenValidated = c =>
            //    {
            //        Console.WriteLine("User successfully authenticated");
            //        return Task.CompletedTask;
            //    },
            //    OnAuthenticationFailed = c =>
            //    {
            //        c.NoResult();
            //        c.Response.ContentType = "text/plain";
            //        if (isDevelopment)
            //        {
            //            return c.Response.WriteAsync(c.Exception.ToString());
            //        }
            //        return c.Response.WriteAsync("An error occured processing your authentication.");
            //    }
            //};
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
