using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TempAuthCookie";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                // Se è una chiamata API rispondi 401
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                // Se l'utente è sulla root o su Home -> vai a Login
                if (ctx.Request.Path == "/" || ctx.Request.Path.StartsWithSegments("/Home"))
                {
                    ctx.Response.Redirect(options.LoginPath);
                }
                else
                {
                    // In tutti gli altri casi non loggato -> AccessDenied
                    ctx.Response.Redirect(options.AccessDeniedPath);
                }

                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
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

//Questo pezzo serve a visuallizare Xml per i commmenti dello swagger(è super opzionale nel caso lo tolgiamo) 
builder.Services.AddSwaggerGen(c =>
{
    // Imposta il percorso del file XML dei commenti
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Includi i commenti XML nello Swagger
    c.IncludeXmlComments(xmlPath);

  
});


var app = builder.Build();

// Ripulisco i cookie all'avvio in modo che il browser se li tiene in cache non crea problemi
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        // Verifica se ? una nuova sessione o mancano cookie
        if (!context.Request.Cookies.ContainsKey("TempAuthCookie") ||
            context.Request.Path == "/Home/Index")
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Headers["Cache-Control"] = "no-cache, no-store";
        }
    }
    await next();
});

//Per far si che quando lancio l'applicazione mi apra la finestra su login

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