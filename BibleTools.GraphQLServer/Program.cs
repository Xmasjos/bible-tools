using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace BibleTools.GraphQLServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configuration.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    configuration.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
#if DEBUG
                    webBuilder.UseKestrel(kestrelOptions =>
                    {
                        // kestrelOptions.EnableAltSvc = true;
                        kestrelOptions.Limits.MaxConcurrentConnections = null;
                        kestrelOptions.Limits.MaxConcurrentUpgradedConnections = null;

                        kestrelOptions.Listen(
                            address: IPAddress.Any, // Enable docker
                            port: 5000, //TODO: Get from config / env
                            listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;//AndHttp3;
                            }
                        );

                        kestrelOptions.Listen(
                            address: IPAddress.Any, // Enable docker
                            port: 5001, //TODO: Get from config / env
                            listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;//AndHttp3;
                                listenOptions.UseHttps();
                            }
                        );
                    });
#else
                    webBuilder.UseIISIntegration();
#endif
                    webBuilder.UseStartup<Startup>();
                });
    }
}
