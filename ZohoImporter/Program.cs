using DataImporter.Framework.Data;
using DataImporter.Framework.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DataImporter.Framework.Repository;
using DataImporter.Framework;

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

            services.AddTransient<IEmailSender, SMSEmailSender>();

            services.AddDbContext<ZohoCRMDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ZohoCRMConnection")));


            services.AddSingleton<IZohoCRMDataRepository, ZohoCRMDbRepository>();


            var provider = services.BuildServiceProvider();

            var importer = new PartnerPortalImporter(provider.GetService<IZohoCRMDataRepository>());
            importer.test();


            var emailsender = provider.GetService<IEmailSender>();

            emailsender.SendEmailAsync("test", "test", "test");

            Console.ReadLine();
            //

        }

       
    }
}