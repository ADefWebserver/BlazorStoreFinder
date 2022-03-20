using AzureMapsControl.Components;
using BlazorStoreFinder;
using BlazorStoreFinder.Data;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BlazorStoreFinderContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
x => x.UseNetTopologySuite()));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Add Syncfusion
builder.Services.AddSyncfusionBlazor(options => { options.IgnoreScriptIsolation = true; });

// Configure the Darnton Geolocation component
builder.Services
    .AddScoped<Darnton.Blazor.DeviceInterop.Geolocation.IGeolocationService,
    Darnton.Blazor.DeviceInterop.Geolocation.GeolocationService>();

// Get the Azure Maps settings from appsettings.json
var AzureMaps = builder.Configuration.GetSection("AzureMaps");

// This code configures anonymous authentication
// for the Azure Maps API
// An auth token will be reuired to access the maps
builder.Services.AddAzureMapsControl(
    configuration => configuration.ClientId = AzureMaps.GetValue<string>("ClientId"));

var app = builder.Build();

// Initialize the AuthService so the later calls to 
// GetAccessToken will work
BlazorStoreFinder.AuthService.SetAuthSettings(AzureMaps);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
