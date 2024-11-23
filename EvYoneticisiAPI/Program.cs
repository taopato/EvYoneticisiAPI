using EvYoneticisiAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// **1. Veritaban� Ba�lant�s�**
builder.Services.AddDbContext<EvYoneticisiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// **2. JWT Ayarlar�**
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Do�rulama Hatas�: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token do�ruland�.");
                return Task.CompletedTask;
            }
        };
    });

// **3. CORS Ayarlar�**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// **4. Swagger ve Controller'lar**
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// **5. Veritaban� Ba�lant�s�n� Test Et**
try
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    using (var context = serviceProvider.GetRequiredService<EvYoneticisiContext>())
    {
        var canConnect = context.Database.CanConnect();
        Console.WriteLine($"Veritaban�na ba�lanabiliyor mu? {canConnect}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Veritaban� ba�lant� hatas�: {ex.Message}");
}

// **6. Kestrel Ayarlar�**
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTPS ba�lant�s� i�in
    options.ListenAnyIP(7008, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS portu
    });

    // HTTP ba�lant�s� i�in
    options.ListenAnyIP(5000); // HTTP portu
});

// **7. Middleware'ler**
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
