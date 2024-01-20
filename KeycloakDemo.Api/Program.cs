using KeycloakDemo.Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;
builder.Services.Configure<AuthenticationOptions>(config.GetSection("Authentication"));
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

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
                            .WithOrigins("http://localhost:5000",
                                              "http://localhost:5001")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                      });
});

// identity.keycloak:8080

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.UseAuthentication();
app.UseCors(AllowSpecificOrigins);
app.UseAuthorization();

app.MapControllers();

app.Run();
