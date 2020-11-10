using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Emporos.Core.Data;
using Emporos.Core.Identity;
using Emporos.Data;
using Emporos.Data.Context;
using Emporos.Middleware;
using Emporos.Services; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Emporos.Api
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup class constructor
        /// </summary>
        /// <param name="configuration"> Configuration params</param>
        /// <param name="envService">Environment definition</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment envService)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(envService.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{envService.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();

            CurrentEnvironment = envService;
        }

        private IWebHostEnvironment CurrentEnvironment { get; set; }
        /// <summary>
        /// Configuration store
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddDbContext<IdentityDbContext>(opt =>
            {
                opt.UseSqlServer(Configuration.GetConnectionString("EmporosDb"));
                if (CurrentEnvironment.IsDevelopment()) opt.EnableSensitiveDataLogging();
            });

            services.AddDbContext<EmporosContext>(opt =>
            {
                opt.UseSqlServer(Configuration.GetConnectionString("EmporosDb"));
                if (CurrentEnvironment.IsDevelopment()) opt.EnableSensitiveDataLogging();
            });

            var appSettings = new AppSettings();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            Configuration.GetSection("AppSettings").Bind(appSettings);

            services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 5;
                opt.Password.RequireNonAlphanumeric = false;
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<IdentityDbContext>().AddDefaultTokenProviders();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = appSettings.JWT.Issuer,
                    ValidAudience = appSettings.JWT.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JWT.Key))
                };
            });

            services.AddTransient<IDataSeeder, DataSeeder>();
            services.AddScoped<UserManager<ApplicationUser>>();
            services.AddScoped<RoleManager<IdentityRole>>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<UserHelper>();

            services.AddHttpContextAccessor();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(appSettings.Swagger.Name,
                    new OpenApiInfo()
                    {
                        Title = appSettings.Swagger.Title,
                        Description = appSettings.Swagger.Description
                    });

                setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Bearer handler",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                },
                                Name = "Bearer",
                                In = ParameterLocation.Header
                            },
                            new List<string>()
                        }
                    });

                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
                setupAction.IncludeXmlComments(xmlCommentsFilePath);
            });

            services.AddWebSocketManager();
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            app.UseWebSocketServer();

            app.UseCors(x => 
                x.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/EmporosApiSpecification/swagger.json",
                    "Emporos Api");
                setupAction.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
