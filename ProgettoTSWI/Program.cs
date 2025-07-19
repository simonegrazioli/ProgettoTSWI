using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //questa line assegna al Cookie un nome
        options.Cookie.Name = "TempAuthCookie";
        //indica dove deve essere reindirizzato l'utente se prova ad andare su una pagina dove
        //dovrebbe essere loggato senza esserlo: 
        // UserLogin è il nome del contreller se è dvisero allora bisogna cambiarlo!!
        options.LoginPath = "/UserLogin/Login";
        //se invece non ha i permessi giusti viene mandato qui
        options.AccessDeniedPath = "/Home/AccessDenied";
    }

    );

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
