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
            ValidateLifetime = true, // Token s�resinin do�rulanmas�n� etkinle�tir
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero // Token s�resinin do�rulu�unu tam olarak kontrol etmek i�in
        };

        // React Native uygulamalar� i�in detayl� hata mesajlar�n� etkinle�tir
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
        policy.AllowAnyOrigin() // Herhangi bir kaynaktan gelen isteklere izin ver
              .AllowAnyMethod() // GET, POST, PUT, DELETE gibi t�m HTTP metodlar�na izin ver
              .AllowAnyHeader(); // Herhangi bir HTTP ba�l���na izin ver
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

var app = builder.Build();

// **6. Hata Y�netimi ve Swagger**
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // �retim ortam� i�in genel hata yakalama
    app.UseExceptionHandler("/error");
}

// **7. Middleware'ler**
app.UseCors("AllowAll"); // CORS politikas�
app.UseAuthentication(); // JWT do�rulama
app.UseAuthorization();  // Yetkilendirme

app.MapControllers(); // Controller'lar i�in route yap�land�rmas�

app.Run();
