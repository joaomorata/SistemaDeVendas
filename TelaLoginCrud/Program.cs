using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TelaLoginCrud.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TelaLoginContextConnection") ?? throw new InvalidOperationException("Connection string 'TelaLoginContextConnection' not found.");

builder.Services.AddDbContext<TelaLoginContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<TelaLoginContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Run();
