
using BankSim.Actions;
using BankSim.Models.Database;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Utilities.Initialize(builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICustomer, Customer>();
builder.Services.AddScoped<IAccount, Account>();
builder.Services.AddScoped<IUser, User>();
builder.Services.AddScoped<ITransaction, Transaction>();
builder.Services.AddScoped<ICipClient, CipClient>();
builder.Services.AddScoped<TransactionStatus>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthenticationCore();
builder.Services.AddBlazoredLocalStorage();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var client = new BankSimDbContext())
{
    //client.Database.EnsureCreated(); //No migration | No update
    await client.Database.MigrateAsync(); //Uses Migration, therefore it updates
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


app.UseAuthentication();
app.UseAuthorization();


app.Run();
