using BlazorApp.Client.Pages;
using BlazorApp.Components;
using BlazorApp.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


var app = builder.Build();

// Automatic migration
using (var scope = app.Services.CreateScope())
{
    ILogger<Program> Logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!dbContext.Database.EnsureCreated())
    {
        var appliedMigrations = dbContext.Database.GetAppliedMigrations();
        if (appliedMigrations.Count() > 0)
        {
            string lastMigration = appliedMigrations.Last();
            Logger.LogInformation($"Last applied migration: {lastMigration}");
        }

        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        if (pendingMigrations.Count() > 0)
        {
            // MIGRAZIONE
            dbContext.Database.Migrate();
        }
    }
    else
    {
        Logger.LogInformation($"Create new database");
        var netCoreVer = System.Environment.Version;
        var historyRepository = dbContext.GetService<IHistoryRepository>();
        dbContext.Database.ExecuteSqlRaw(historyRepository.GetCreateIfNotExistsScript());
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        foreach (var migration in pendingMigrations)
        {
            var script = historyRepository.GetInsertScript(new HistoryRow(migration, netCoreVer.ToString()));
            dbContext.Database.ExecuteSqlRaw(script);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp.Client._Imports).Assembly);

app.Run();
