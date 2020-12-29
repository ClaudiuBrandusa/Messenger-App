using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Messenger_API.Data;
using Messenger_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Messenger_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost webHost = CreateHostBuilder(args).Build();

            using (var scope = webHost.Services.CreateScope())
            {
                IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                UserContext userContext = scope.ServiceProvider.GetRequiredService<UserContext>();
                MessageContext messageContext = scope.ServiceProvider.GetRequiredService<MessageContext>();

                var users = new List<SmallUser>();
                foreach(var user in userContext.Users)
                {
                    users.Add(new SmallUser 
                    {
                        UserId = user.Id,
                        UserName = user.UserName
                    });
                }
                var smallUsers = messageContext.SmallUsers.AsNoTracking().ToList();

                var results = users.Where(u => !smallUsers.Any(s => u.UserId.Equals(s.UserId) && u.UserName.Equals(s.UserName))); 
                
                foreach(var user in results)
                {
                    messageContext.SmallUsers.Add(user);
                }

                await messageContext.SaveChangesAsync();
            }

            await webHost.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();
                    webBuilder.UseUrls($"http://localhost:{config.GetValue<string>("Port")}");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
