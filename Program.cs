using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Authentication
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://sts.windows.net/{builder.Configuration["AzureAd:TenantId"]}/",
            ValidateAudience = true,
            ValidAudience = "api://51e3ec1a-fedc-4669-8fd6-9cccd21a7dbeaca",
            ValidateLifetime = true
        };
    });*/
/*builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Allow your React app's origin
              .AllowAnyHeader() // Allow all headers
              .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
              .AllowCredentials(); // Allow credentials if needed
    });
});*/

/*builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://your-auth-server"; // Your token issuer
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Optional: Set to true if audience validation is required
            ValidateIssuer = true,   // Validate token issuer
            ValidIssuer = "your-issuer" // Replace with your issuer
        };
    });*/


// Add authentication and configure Azure AD
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://sts.windows.net/{builder.Configuration["AzureAd:TenantId"]}/",
            ValidateAudience = true,
            ValidAudience = "api://51e3ec1a-fedc-4669-8fd6-9cccd21a7dbeaca",
            ValidateLifetime = true,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            //RoleClaimType = "roles" // Specify the claim containing roles
        };
    }); */
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReadScope", policy =>
        policy.RequireAssertion(context =>
        {
            var scopes = context.User.FindFirst("scp")?.Value.Split(' ');
            return scopes != null && scopes.Contains("read");
        }));

    options.AddPolicy("ReadWriteScope", policy =>
        policy.RequireAssertion(context =>
        {
            var scopes = context.User.FindFirst("scp")?.Value.Split(' ');
            return scopes != null && scopes.Contains("read-write");
        }));
});



// Add Swagger with Bearer authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseSession();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
