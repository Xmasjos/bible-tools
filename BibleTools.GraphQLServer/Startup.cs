using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using HotChocolate.AspNetCore.Voyager;
using HotChocolate.Types;


namespace BibleTools.GraphQLServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var gqlServer = services.AddGraphQLServer();

            var gqlTypes = Assembly.GetAssembly(GetType())?.DefinedTypes
                .Where(t => typeof(ITypeSystemObject).IsAssignableFrom(t.AsType()) || typeof(IInterfaceType).IsAssignableFrom(t.AsType()))
                .Where(t => t.IsClass)
                .Where(t => t.IsAbstract == false)
                .ToArray();

            gqlServer.ConfigureSchema(schemaBuilder =>
            {
                schemaBuilder.ModifyOptions(op =>
                {
                    op.DefaultBindingBehavior = BindingBehavior.Explicit;
                    op.SortFieldsByName = true;
                });

                if (gqlTypes != null)
                    schemaBuilder.AddTypes(gqlTypes);
            });

            gqlServer.AddQueryType();
            // gqlServer.AddMutationType();

            gqlServer.AddApolloTracing();
            gqlServer.InitializeOnStartup();

            gqlServer.ModifyRequestOptions(options =>
            {
                options.IncludeExceptionDetails = true;
            });

            gqlServer.OnSchemaError((context, error) =>
            {
                //TODO: Write out error
                Console.WriteLine("Schema error!");
            });

            services.AddCors();
        }

        private static string CombineUri(params string[] uriParts)
            => $"/{string.Join('/', uriParts.Select(t => t.Trim('/')))}";

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // app.UseAuthentication();
            // app.UseAuthorization();

            //TODO: GraphQL Configuration
            var gqlBasePath = "/graphql";
            var gqlPath = CombineUri(gqlBasePath, "v1");
            var gqlOptions = new GraphQLServerOptions()
            {
                AllowedGetOperations = AllowedGetOperations.Query,
                EnableGetRequests = true,
                EnableSchemaRequests = true,
            };

            gqlOptions.Tool.Enable = Debugger.IsAttached;
            gqlOptions.Tool.HttpMethod = DefaultHttpMethod.Get;

            app.UseVoyager(gqlPath, CombineUri(gqlBasePath, "voyager"));

            if (env.IsDevelopment())
            {
                app.UseGraphiQL(gqlPath, CombineUri(gqlBasePath, "graphiql"));
                app.UsePlayground(new PlaygroundOptions()
                {
                    Path = CombineUri(gqlBasePath, "playground"),
                    QueryPath = gqlPath,
                    SubscriptionPath = gqlPath,
                    EnableSubscription = true,
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL(gqlPath)
                    .WithOptions(gqlOptions);

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
