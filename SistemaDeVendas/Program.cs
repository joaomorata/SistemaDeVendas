using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaDeVendas.Areas.Identity.Data;
using SistemaDeVendas.Services;

// Npgsql: mantem o comportamento de DateTime sem timezone (como era no SQL Server).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Em producao (Render/Supabase) a conexao chega como URL: postgresql://user:senha@host:porta/banco
// O Npgsql nao aceita esse formato, entao convertemos para o formato "chave=valor".
// Prioridade: DATABASE_URL (env) > ConnectionStrings:SistemaDeVendasContextConnection (env/appsettings/user-secrets).
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = !string.IsNullOrWhiteSpace(databaseUrl)
    ? BuildNpgsqlConnectionString(databaseUrl)
    : builder.Configuration.GetConnectionString("SistemaDeVendasContextConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Connection string nao encontrada. Defina a variavel de ambiente DATABASE_URL " +
        "ou ConnectionStrings__SistemaDeVendasContextConnection.");

static string BuildNpgsqlConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        Database = uri.AbsolutePath.Trim('/'),
        SslMode = SslMode.Require
    };

    return csb.ConnectionString;
}

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
