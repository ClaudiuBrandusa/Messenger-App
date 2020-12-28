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

                //Create source connection
                SqlConnection source = new SqlConnection(configuration.GetConnectionString("UserContextConnection"));
                //Create destination connection
                SqlConnection destination = new SqlConnection(configuration.GetConnectionString("MessageContextConnection"));
                // Clean up destination table. Your destination database must have the
                // table with schema which you are copying data to.
                // Before executing this code, you must create a table BulkDataTable
                // in your database where you are trying to copy data to.
                SqlCommand cmd = new SqlCommand("DELETE FROM SmallUsers", destination);
                //Open source and destination connections
                source.Open();
                destination.Open();
                cmd.ExecuteNonQuery();
                //Select data from AspNetUsers table
                cmd = new SqlCommand("SELECT Id, UserName FROM AspNetUsers", source);
                //Execute reader
                SqlDataReader reader = cmd.ExecuteReader();
                //Create SqlBulkCopy
                SqlBulkCopy bulkData = new SqlBulkCopy(destination);
                //Set destination table name
                bulkData.DestinationTableName = "SmallUsers";
                //Write data
                bulkData.WriteToServer(reader);
                //Close objects
                bulkData.Close();
                destination.Close();
                source.Close();
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
