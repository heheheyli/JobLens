using Azure;
using Azure.AI.TextAnalytics;
using JobLens.Web.Data;
using JobLens.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();  
builder.Services.AddDbContext<JobLensDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var languageEndpoint = builder.Configuration["AzureLanguage:Endpoint"];
var languageKey = builder.Configuration["AzureLanguage:Key"];
if (!string.IsNullOrWhiteSpace(languageEndpoint) && !string.IsNullOrWhiteSpace(languageKey))
{
    builder.Services.AddSingleton(new TextAnalyticsClient(new Uri(languageEndpoint), new AzureKeyCredential(languageKey)));
    builder.Services.AddScoped<ISkillExtractor, AzureSkillExtractor>();
}
else
{
    builder.Services.AddScoped<ISkillExtractor, UnconfiguredSkillExtractor>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<JobLensDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

    app.MapHealthChecks("/health");

app.Run();

// Lets the integration tests start the whole app with WebApplicationFactory<Program>.
public partial class Program { }
