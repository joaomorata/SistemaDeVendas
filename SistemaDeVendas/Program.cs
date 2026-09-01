using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Areas.Identity.Data;
using SistemaDeVendas.Services;

// Npgsql: mantem o comportamento de DateTime sem timezone (como era no SQL Server).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SistemaDeVendasContextConnection") ?? throw new InvalidOperationException("Connection string 'SistemaDeVendasContextConnection' not found.");

builder.Services.AddDbContext<SistemaDeVendasContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<SistemaDeVendasContext>();

builder.Services.AddHttpClient<GeocodingService>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "SistemaDeVendas");
});

builder.Services.AddScoped<OtimizadorRota>();

builder.Services.AddControllersWithViews();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SistemaDeVendasContext>();
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

app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();
