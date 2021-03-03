using System.Globalization;
using System.Linq;
using Infra.Net.SwaggerFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Modelo.Versionamento_API
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
            services.AddApiVersioning(o => {
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
                .AddApiExplorer();

            services.ConfigureSwaggerGen(x => { });

            services.AddSwaggerGen(c => {
                c.CustomSchemaIds(x => x.FullName);
                c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "Template Versioning_Api", Version = "v1.0" });
                c.SwaggerDoc("v2.0", new OpenApiInfo { Title = "Template Versioning_Api", Version = "v2.0" });

                c.OperationFilter<RemoveVersionParameterFilter>();
                c.OperationFilter<AttachControllerNameFilter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
                c.SwaggerGeneratorOptions.ConflictingActionsResolver = x => x.First();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.EnableAnnotations();
            })
            .AddSwaggerGenNewtonsoftSupport(); // explicit opt-in - needs to be placed after AddSwaggerGen()

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
            });
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
                app.UseHsts();
                pathBase = (Configuration["AppConfig:PathBase"] ?? "").Split(",")[0] + "/";
            }

            app.UseHttpsRedirection();
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
                    c.SwaggerEndpoint($"/{pathBase}api-docs/v1.0/swagger.json", "Template Versioning_Api V1");
                    c.SwaggerEndpoint($"/{pathBase}api-docs/v2.0/swagger.json", "Template Versioning_Api V2");
                });
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        }
    }
}
