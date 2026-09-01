using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using TelaLoginCrud.Areas.Identity.Data;
using TelaLoginCrud.Services;

// Npgsql: mantem o comportamento de DateTime sem timezone (como era no SQL Server).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TelaLoginContextConnection") ?? throw new InvalidOperationException("Connection string 'TelaLoginContextConnection' not found.");

builder.Services.AddDbContext<TelaLoginContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<TelaLoginContext>();

builder.Services.AddHttpClient<GeocodingService>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "TelaLoginCrud");
});

builder.Services.AddScoped<OtimizadorRota>();

builder.Services.AddControllersWithViews();

// Render/Heroku e afins terminam o TLS no proxy: confia nos headers X-Forwarded-*.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Aplica as migrations pendentes no startup (cria o schema no primeiro deploy).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TelaLoginContext>();
    db.Database.Migrate();
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Endpoint leve pra health check / manter o serviço acordado.
app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();
