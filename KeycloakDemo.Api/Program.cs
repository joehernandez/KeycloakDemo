using KeycloakDemo.Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;
builder.Services.Configure<AuthenticationOptions>(config.GetSection(AuthenticationOptions.SectionName));
builder.Services.AddAuthenticationWithJwt(config, builder.Environment.IsDevelopment());

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
