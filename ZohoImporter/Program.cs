using DataImporter.Framework.Data;
using DataImporter.Framework.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DataImporter.Framework.Repository;
using DataImporter.Framework;
using System.Threading.Tasks;
using System.Threading;
using DataImporter.Framework.Models;
using DataImporter.Framework.Extensions;
using Website.Extensions;
using Microsoft.Extensions.Logging;
using NLog;

namespace ZohoImporter
{
    

    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("CORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

            var configuration = builder.Build();
            
            //
            var services = new ServiceCollection();

            services.AddDbContext<ZohoCRMDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ZohoCRMConnection")));

            //services.AddDbContext<ACLDbContext>(options =>
            //{
            //    options.UseSqlServer(configuration.GetConnectionString("ACLConnection"));

            //});

            //services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            //{
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;

            //})
            //    .AddEntityFrameworkStores<ACLDbContext>()
            //    .AddUserStore<SMSUserStore<ApplicationUser>>()
            //    .AddUserManager<SMSUserManager<ApplicationUser>>()
            //    .AddDefaultTokenProviders();


            services.Configure<SMTPOptions>(configuration.GetSection("SMTPSettings"));
            services.AddSingleton<IEmailSender, SMSEmailSender>();

            services.Configure<MyobImportOptions>(configuration.GetSection("MyobImportOptions"));
            services.AddSingleton<DataImporter.Framework.Services.MyobApiService>();

            services.AddScoped<IZohoCRMDataRepository, ZohoCRMDbRepository>();

            //services.AddSingleton<IZohoCRMDataRepository, ZohoCRMDbRepository>();


            var provider = services.BuildServiceProvider();

            var importer = new ZohoImportManager(provider.GetService<MyobApiService>(), provider.GetService<IZohoCRMDataRepository>(),
                    provider.GetService<IEmailSender>(), configuration.GetValue<string>("ZohoToken"));

            importer.DisplayMessage += Importer_DisplayMessage;

            var cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;

            int intervalSeconds = configuration.GetValue<int>("ImportDelay", 30);

            var starter = StartImportTask(importer, ct, intervalSeconds);

            DisplayMessage("Start.....");

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLower().Trim();
                    if (text == "quit")
                    {
                        DisplayMessage("Quiting.....");
                        cts.Cancel();
                        importer.StopImportAsync().Wait(ct);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }

        private static async Task StartImportTask(ZohoImportManager importer, CancellationToken token, int intervalSeconds = 30)
        {
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    await importer.StartImportAsync();

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    Thread.Sleep(intervalSeconds * 1000);
                }

            });
        }

        private static void Importer_DisplayMessage(object sender, MessageEventArgs e)
        {
            if (e is MessageEventArgs mea)
            {
                DisplayMessage(mea.Message);
            }
        }

        private static void DisplayMessage(string message)
        {
            Logger.Info(message);
            Console.WriteLine("[{0:yyyy MM dd HH:mm:ss}]: {1}", DateTime.Now, message);
        }

    }
}