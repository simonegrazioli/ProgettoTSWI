using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient();

//builder.Services.AddHttpContextAccessor(); 

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TempAuthCookie";
        //indica dove deve essere reindirizzato l'utente se prova ad andare su una pagina dove
        //dovrebbe essere loggato senza esserlo: 
        // UserLogin è il nome del contreller se è dvisero allora bisogna cambiarlo!!

        //options.LoginPath = "/UserLogin/Login";

        //se invece non ha i permessi giusti viene mandato qui
        //options.AccessDeniedPath = "/Home/AccessDenied";

        options.LoginPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/Index"; //se accesso negato reindirizzo a index per effettuare l'accesso
    });
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//ripulisco i cookie all'avvio in modo che il browser se li tiene in cache non crea problemi
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        // Verifica se è una nuova sessione o mancano cookie
        if (!context.Request.Cookies.ContainsKey("TempAuthCookie") ||
            context.Request.Path == "/Home/Index")
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Headers["Cache-Control"] = "no-cache, no-store";
        }
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Admin",
    pattern: "Admin/{action}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
