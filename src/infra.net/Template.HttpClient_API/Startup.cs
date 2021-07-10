using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Newtonsoft.Json;
using Template.HttpClient_API.Controllers;
using Template.HttpClient_API.HttpServices;
using Infra.Net.LogManager;
using Infra.Net.LogManager.WebExtensions;
using Infra.Net.DataAccess.MongoDb.Repository;
using Infra.Net.DataAccess.MongoDb.Service;
using Infra.Net.DataAccess.MongoDb.WebHelpers;
using Infra.Net.SwaggerFilters;

namespace Template.HttpClient_API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Host { get; set; }
        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(env.ContentRootPath)
                .AddConfiguration(config)
                //Add other settings.json files as needed
                //.AddJsonFile("serilogsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = configBuilder.Build();
            Host = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                //.AddJsonOptions(x =>
                //{
                //    x.JsonSerializerSettings.WriteIndented = Host.IsDevelopment();
                //    x.JsonSerializerSettings.MaxDepth = 64;
                //    x.JsonSerializerSettings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                //    x.JsonSerializerSettings.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                //    x.JsonSerializerSettings.PropertyNameCaseInsensitive = true;
                //    x.JsonSerializerSettings.AllowTrailingCommas = true;
                //})
                ;

            services.AddSingleton(x => Configuration);

            services.AddApiVersioning();
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.Conventions.Add(new VersionByNamespaceConvention());
                o.ApiVersionReader = ApiVersionReader.Combine(
                    //new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("v"),
                    new HeaderApiVersionReader("api-version"));
            });

            services.AddMvcCore()
                .AddControllersAsServices()
                .AddApiExplorer()
                ;

            services.ConfigureSwaggerGen(x => { });

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.FullName);
                c.SwaggerDoc("v1.0", new OpenApiInfo {Title = "Template HttpClient", Version = "v1.0"});
                c.SwaggerGeneratorOptions.ConflictingActionsResolver = x => x.First();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basicAuth" }
                        },
                        new string[]{}
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[]{}
                    }
                });

                c.EnableAnnotations();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                //This has to be the last one in swaggerGen in order to render correctly summaries and remarks
                c.OperationFilter<RemoveVersionParameterFilter>();
                c.OperationFilter<AttachControllerNameFilter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
            });
            //.AddSwaggerGenNewtonsoftSupport(); // explicit opt-in - needs to be placed after AddSwaggerGen()

            HttpClientConfiguration.Configure(services, Configuration);

            services.AddScopedWithLog<IMongoService<TestEntity>, MongoHttpService<TestEntity>>();
            services.AddScopedWithLog<IMongoReadOnlyService<TestEntity>, MongoHttpReadOnlyService<TestEntity>>();
            services.AddScopedWithLog<IMongoHttpReadOnlyService<TestEntity>, MongoHttpReadOnlyService<TestEntity>>();
            services.AddScopedWithLog<IMongoHttpService<TestEntity>, MongoHttpService<TestEntity>>();
            services.AddScopedWithLog<IMongoRepository<TestEntity>, TestRepository>();

            services.AddScopedWithLog<IFSService, FSService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            string pathBase;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                pathBase = string.Empty;
            }
            else
            {
                pathBase = (Configuration["AppConfig:PathBase"] ?? "").Split(",")[0] + "/";
                app.UseHsts();
            }
            //TODO Unhandled exception middleware
            app.UseLogMiddleware();
            app.UseHttpsRedirection();
            app.UseHeaderPropagation();
            app.UseForwardedHeaders();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors("AllowAllPolicy");

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
            })
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/{pathBase}api-docs/v1.0/swagger.json", "Template HttpClient V1");
                });

            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        }
    }
}
