using System.Globalization;
using Microsoft.AspNetCore.Localization;
using OrderDiscount.Application.Configuration;
using OrderDiscount.Application.Interfaces;
using OrderDiscount.Application.Services;
using OrderDiscount.Application.Strategies;
using OrderDiscount.Domain.Interfaces;
using OrderDiscount.Infrastructure.Authentication;
using OrderDiscount.Infrastructure.Persistence;
using OrderDiscount.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, services, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(services));

builder.Services.Configure<DiscountSettings>(builder.Configuration.GetSection(DiscountSettings.SectionName));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SectionName));

builder.Services.AddSingleton<IDiscountStrategy, RegularDiscountStrategy>();
builder.Services.AddSingleton<IDiscountStrategy, PremiumDiscountStrategy>();
builder.Services.AddSingleton<IDiscountStrategy, VipDiscountStrategy>();
builder.Services.AddSingleton<IDiscountStrategyResolver, DiscountStrategyResolver>();
builder.Services.AddScoped<IOrderDiscountService, OrderDiscountService>();

builder.Services.AddConfigurablePersistence(builder.Configuration);
builder.Services.AddConfigurableAuth(builder.Configuration);
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews();

var app = builder.Build();
var cultures = new[] { new CultureInfo("en-US"), new CultureInfo("de-DE") };
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("en-US"), SupportedCultures = cultures, SupportedUICultures = cultures });

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }