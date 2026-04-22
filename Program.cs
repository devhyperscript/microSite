using firstproject.Areas.Identity.Data;
using firstproject.Data;
using firstproject.Helpers;
using firstproject.Models.BusinessLayer;
using firstproject.Models.DatabaseLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ DB Connection
var connectionString = builder.Configuration.GetConnectionString("AppDbContextConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ✅ Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // 🔥 temp off (debug ke liye)
})
.AddEntityFrameworkStores<AppDbContext>();

// ✅ JWT Setup
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // 🔥 VERY IMPORTANT (HTTP use kar rahe ho)
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // 🔥 token expiry exact check
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey))
    };
});

// ✅ 🔥 CORS FIX (IMPORTANT CHANGE)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true) // 🔥 sab allow (debug ke liye best)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 🔥 important
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDatabaseLayer, DatabaseLayer>();
builder.Services.AddScoped<IBusinessLayer, BusinessLayer>();
builder.Services.AddScoped<JwtHelper>();

var app = builder.Build();

// ❌ HSTS disable for now (HTTP testing)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); ❌ comment
}

app.UseRouting();

// ❗ IMPORTANT ORDER (bahut log yaha galti karte hain)
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();