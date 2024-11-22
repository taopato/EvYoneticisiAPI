using EvYoneticisiAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// **1. Veritabaný Baðlantýsý**
builder.Services.AddDbContext<EvYoneticisiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// **2. JWT Ayarlarý**
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Token süresinin doðrulanmasýný etkinleþtir
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero // Token süresinin doðruluðunu tam olarak kontrol etmek için
        };

        // React Native uygulamalarý için detaylý hata mesajlarýný etkinleþtir
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Doðrulama Hatasý: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token doðrulandý.");
                return Task.CompletedTask;
            }
        };
    });

// **3. CORS Ayarlarý**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Herhangi bir kaynaktan gelen isteklere izin ver
              .AllowAnyMethod() // GET, POST, PUT, DELETE gibi tüm HTTP metodlarýna izin ver
              .AllowAnyHeader(); // Herhangi bir HTTP baþlýðýna izin ver
    });
});

// **4. Swagger ve Controller'lar**
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// **5. Veritabaný Baðlantýsýný Test Et**
try
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    using (var context = serviceProvider.GetRequiredService<EvYoneticisiContext>())
    {
        var canConnect = context.Database.CanConnect();
        Console.WriteLine($"Veritabanýna baðlanabiliyor mu? {canConnect}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Veritabaný baðlantý hatasý: {ex.Message}");
}

var app = builder.Build();

// **6. Hata Yönetimi ve Swagger**
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Üretim ortamý için genel hata yakalama
    app.UseExceptionHandler("/error");
}

// **7. Middleware'ler**
app.UseCors("AllowAll"); // CORS politikasý
app.UseAuthentication(); // JWT doðrulama
app.UseAuthorization();  // Yetkilendirme

app.MapControllers(); // Controller'lar için route yapýlandýrmasý

app.Run();
