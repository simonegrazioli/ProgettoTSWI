using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // Per OpenApiInfo
using ProgettoTSWI.Data;
using System.Reflection; // Per Assembly


var builder = WebApplication.CreateBuilder(args);


// Aggiungi servizi Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProgettoTSWI API",
        Version = "v1",
        Description = "API per la gestione utenti",
        Contact = new OpenApiContact
        {
            Name = "Supporto",
            Email = "supporto@progetto.it"
        }
    });

    // Aggiungi supporto per i commenti XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Aggiungi supporto per gli attributi [Authorize]
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }
    ));

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

// Aggiungi prima di builder.Build()
BCrypt.Net.BCrypt.EnhancedHashPassword("", 11, BCrypt.Net.HashType.SHA256);

var app = builder.Build();

// Abilita Swagger in sviluppo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProgettoTSWI v1");
        c.RoutePrefix = "api-docs"; // Accessibile su /api-docs
    });
}

app.UseStaticFiles();

app.Use(async (context, next) => {
    // Debug: stampa tutte le richieste API
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        Console.WriteLine($"API Request: {context.Request.Method} {context.Request.Path}");
    }
    await next();
});





app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Questo è fondamentale
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();