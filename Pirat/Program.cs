using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Pirat
{
#pragma warning disable CA1052 // Suppress because Program cannot be made "Static" or "NotInheritable"
    public class Program
#pragma warning restore CA1052
    {
        public static void Main(string[] args)
        {
            CheckEnvironmentVariables();
            CreateHostBuilder(args).Build().Run();
        }


        public static void CheckEnvironmentVariables()
        {
            List<string> requiredEnvironmentVariables = new List<string>()
            {
                "PIRAT_SENDER_MAIL_ADDRESS",
                "PIRAT_SENDER_MAIL_PASSWORD",
                "PIRAT_SENDER_MAIL_USERNAME",
                "PIRAT_SENDER_MAIL_SMTP_HOST",
                "PIRAT_INTERNAL_RECEIVER_MAIL",

                "PIRAT_HOST",
                "PIRAT_CONNECTION",
                "PIRAT_GOOGLE_API_KEY",
                "PIRAT_GOOGLE_RECAPTCHA_SECRET"
            };
            foreach (var requiredEnvironmentVariable in requiredEnvironmentVariables)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(requiredEnvironmentVariable)))
                {
                    throw new Exception("Environment variable is missing: " + requiredEnvironmentVariable);
                }
            }
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
