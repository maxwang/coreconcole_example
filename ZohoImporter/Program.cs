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

namespace ZohoImporter
{
    class Program
    {
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


            services.AddTransient<IEmailSender, SMSEmailSender>();
            
            services.AddSingleton<IZohoCRMDataRepository, ZohoCRMDbRepository>();


            var provider = services.BuildServiceProvider();

            var importer = new ZohoImportManager(provider.GetService<IZohoCRMDataRepository>(),
                    provider.GetService<IEmailSender>());

            importer.DisplayMessage += Importer_DisplayMessage;

            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            var task = Task.Factory.StartNew(async () =>
            {
                Console.WriteLine("start task");
                if (token.IsCancellationRequested == true)
                {
                    Console.WriteLine("Task was cancelled before it got started.");
                    token.ThrowIfCancellationRequested();
                }

                await importer.StartImportAsync();

                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("start ccancellation");

                    await importer.StopImportAsync();
                    token.ThrowIfCancellationRequested();
                }

                
                    

            }, token);


            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLower().Trim();
                    if (text == "quit")
                    {
                        tokenSource.Cancel();
                        tokenSource.Dispose();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.ReadLine();


        }

        private static void Importer_DisplayMessage(object sender, MessageEventArgs e)
        {
            MessageEventArgs mea = e as MessageEventArgs;
            if (mea != null)
            {
                Console.WriteLine("[{0:yyyy MM dd HH:mm:ss}]: {1}", DateTime.Now, mea.Message);
            }
        }
               
    }
}