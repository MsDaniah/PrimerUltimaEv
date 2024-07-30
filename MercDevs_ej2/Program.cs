using MercDevs_ej2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure database context
builder.Services.AddDbContext<MercyDeveloperContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("connection"),
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.25-mariadb")
    ));

// Configure email services
builder.Services.AddFluentEmail(builder.Configuration["Email:SenderEmail"])
    .AddSmtpSender(new SmtpClient
    {
        Host = builder.Configuration["Email:SmtpHost"],
        Port = int.Parse(builder.Configuration["Email:SmtpPort"]),
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new System.Net.NetworkCredential(
            builder.Configuration["Email:SmtpUser"],
            builder.Configuration["Email:SmtpPass"]
        )
    });

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Ingresar";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Ingresar}/{id?}");

app.Run();
