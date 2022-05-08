using AzureMapsControl.Components;
using BlazorStoreFinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BlazorStoreFinderContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
x => x.UseNetTopologySuite()));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Get the Azure Maps settings from appsettings.json
var AzureMaps = builder.Configuration.GetSection("AzureMaps");

// Configure the Darnton Geolocation component
builder.Services
.AddScoped<
Darnton.Blazor.DeviceInterop.Geolocation.IGeolocationService,
Darnton.Blazor.DeviceInterop.Geolocation.GeolocationService
>();

// This code configures anonymous authentication
// for the Azure Maps API
// An auth token will be required to access the maps
builder.Services.AddAzureMapsControl(
    configuration => configuration.ClientId = 
    AzureMaps.GetValue<string>("ClientId"));

// Add Syncfusion
builder.Services.AddSyncfusionBlazor();

// Register StoreLocationService
builder.Services.AddScoped<StoreLocationService>();

// Build the application and return the startup type.
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
