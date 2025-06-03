using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropertyGalla.Data;
using PropertyGalla.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with JSON options
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// CORS Policy for Frontend (Localhost or Deploy URL)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Configure EF Core with resilient SQL connection
builder.Services.AddDbContext<PropertyGallaContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PropertyGallaContextConnection"),
        sql => sql.EnableRetryOnFailure()
    ));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Custom Services
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Global Middleware Setup
app.UseDeveloperExceptionPage(); // Optional, for development

app.UseHttpsRedirection();
app.UseStaticFiles();

// Apply CORS (must come before routing/auth)
app.UseCors("FrontendPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint Mapping
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
