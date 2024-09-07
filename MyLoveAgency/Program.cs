using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS.Core;
using MyLoveAgency.Models;
using MyLoveAgency.Models.Database;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using MyLoveAgency.Application;

namespace MyLoveAgency
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceClass.WriteLog("Запуск приложения! Время: " + DateTime.Now + "\n");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                DataClass.connectionString = builder.Configuration.GetConnectionString("Database");

                builder.Services.AddTransient<LovelyLoveDbContext>();

                builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                });
                builder.Services.AddMvc();

                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
                {
                    options.AccessDeniedPath = "/Page/Home/";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.MaxAge = TimeSpan.FromDays(7);
                });
                builder.Services.AddAuthorization();

                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxRequestLineSize = 100000;
                });

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<LovelyLoveDbContext>();

                    if (!ServiceClass.GetPassword()) throw new Exception("The password could not be set!");

                    if (ServiceClass.GetMailing() <= 0) throw new Exception("The mailing data was not set!");

                    if (!ServiceClass.GetEmailForNotifications()) throw new Exception("The email for notifications was not set!");

                    // Чтение данных из таблицы Localization, заполнение буферных массивов
                    try
                    {
                        var localizationTable = dbContext.Localizations.ToList();

                        var slogan = localizationTable.Where(x => x.Name == "Slogan").ToList();
                        HomeModelEnglish.slogan = slogan[0]?.En?.ToString();
                        HomeModelUkrainian.slogan = slogan[0]?.Ua?.ToString();
                        HomeModelPoland.slogan = slogan[0]?.Pl?.ToString();

                        var aboutUs = localizationTable.Where(x => x.Name == "About_us").ToList();
                        HomeModelEnglish.aboutUsText = aboutUs[0]?.En?.ToString();
                        HomeModelUkrainian.aboutUsText = aboutUs[0]?.Ua?.ToString();
                        HomeModelPoland.aboutUsText = aboutUs[0]?.Pl?.ToString();

                        var advantagesTitle = localizationTable.Where(x => x.Name.Contains("Advantage_title")).ToList();
                        foreach (var title in advantagesTitle)
                        {
                            HomeModelEnglish.advantageTitle.Add(title?.En?.ToString());
                            HomeModelUkrainian.advantageTitle.Add(title?.Ua?.ToString());
                            HomeModelPoland.advantageTitle.Add(title?.Pl?.ToString());
                        }

                        var advantagesText = localizationTable.Where(x => x.Name.Contains("Advantage_text")).ToList();
                        foreach (var text in advantagesText)
                        {
                            HomeModelEnglish.advantageText.Add(text?.En?.ToString());
                            HomeModelUkrainian.advantageText.Add(text?.Ua?.ToString());
                            HomeModelPoland.advantageText.Add(text?.Pl?.ToString());
                        }

                        var stagesTitle = localizationTable.Where(x => x.Name.Contains("SO_title")).ToList();
                        foreach (var title in stagesTitle)
                        {
                            HomeModelEnglish.stageTitle.Add(title?.En?.ToString());
                            HomeModelUkrainian.stageTitle.Add(title?.Ua?.ToString());
                            HomeModelPoland.stageTitle.Add(title?.Pl?.ToString());
                        }

                        var stagesText = localizationTable.Where(x => x.Name.Contains("SO_text")).ToList();
                        foreach (var text in stagesText)
                        {
                            HomeModelEnglish.stageText.Add(text?.En?.ToString());
                            HomeModelUkrainian.stageText.Add(text?.Ua?.ToString());
                            HomeModelPoland.stageText.Add(text?.Pl?.ToString());
                        }

                        var feedbackTitle = localizationTable.Where(x => x.Name.Contains("Feedback_title")).OrderBy(x => x.Name).ToList();
                        foreach (var title in feedbackTitle)
                        {
                            HomeModelEnglish.feedbackTitle.Add(title?.En?.ToString());
                            HomeModelUkrainian.feedbackTitle.Add(title?.Ua?.ToString());
                            HomeModelPoland.feedbackTitle.Add(title?.Pl?.ToString());
                        }

                        var feedbackText = localizationTable.Where(x => x.Name.Contains("Feedback_text")).OrderBy(x => x.Name).ToList();
                        foreach (var text in feedbackText)
                        {
                            HomeModelEnglish.feedbackText.Add(text?.En?.ToString());
                            HomeModelUkrainian.feedbackText.Add(text?.Ua?.ToString());
                            HomeModelPoland.feedbackText.Add(text?.Pl?.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Data could not be read from the Localization table!\r\n" + e);
                    }

                    // Чтение данных из таблиц, заполнение буферных массивов
                    try
                    {
                        DataClass.TypeService = dbContext.TypeServices.ToList();
                        DataClass.Services = dbContext.Services.ToList();
                        DataClass.Packages = dbContext.PackageServices.ToList();
                        DataClass.ServiceImages = dbContext.StorageImageServices.ToList();
                        DataClass.GalleryImages = dbContext.StorageImageGalleries.ToList();
                        DataClass.FAQ = dbContext.Faqs.ToList();
                        DataClass.Contacts = dbContext.Contacts.Take(500).ToList();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Data could not be read from the Service table!\r\n" + e);
                    }
                }

                app.UseStaticFiles();
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Page}/{action=Home}/{id?}");

                app.Run();
            }
            catch (Exception e)
            {
                ServiceClass.WriteLog("Критическая ошибка, приложение остановлено! Время: " + DateTime.Now + "\nИнформация: " + e + "\n\r\n\r");
            }
        }
    }
}
