using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sammaron.Authentication.JwtBearer;
using Sammaron.Authentication.JwtBearer.Handlers;
using Sammaron.Core.Data;
using Sammaron.Core.Interfaces;
using Sammaron.Core.Models;
using Sammaron.Core.Services;
using Sammaron.Server.ViewModels;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sammaron.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureOptions(RequestLocalizationOptions options)
        {
            var cultures = new[]
            {
                new CultureInfo("en"), new CultureInfo("ar")
            };
            options.DefaultRequestCulture = new RequestCulture("en", "en");
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

            services.AddDbContext<ApiContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(ConfigureOptions);
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.User = new UserOptions
                    {
                        RequireUniqueEmail = true
                    };
                    options.Password = new PasswordOptions
                    {
                        RequireDigit = false,
                        RequireLowercase = false,
                        RequireNonAlphanumeric = false,
                        RequireUppercase = false,
                        RequiredLength = 6
                    };
                })
                .AddEntityFrameworkStores<ApiContext>()
                .AddDefaultTokenProviders();
                //.AddErrorDescriber<ErrorDescriber>();

            services.AddMvc();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Sammaorn Jwt Authentication Server",
                    Description = "An API that allows",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Rahma Ahmed", Email = "rahmed.sammaron@gmail.com", Url = "https://github.com/rahmadSammaron" },
                    License = new License { Name = "MIT License", Url = "https://github.com/rahmadSammaron/Ionic-3-Visual-Studio-Template/blob/master/LICENSE.md" }
                });
                options.OperationFilter<HeaderFilter>();
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) 
                    .AddScheme<BearerOptions, BearerAuthenticationHandler>("Bearer", null, options =>
                    {
                        options.Audience = "*";
                        options.TokenLifetimeInMinutes = 3600;
                        options.ClaimsIssuer = IdentityConstants.ApplicationScheme;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("SigningKey"))),
                            TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("DecryptionKey"))),
                            ValidIssuer = IdentityConstants.ApplicationScheme,
                            ValidAudience = "*",
                            ClockSkew = TimeSpan.Zero
                        };
                        options.Events = new BearerEvents();
                    });

            services.AddAutoMapper(expression =>
            {
                expression.AddProfile<MappingProfile>();
            });

            services.AddTransient<ISmsSender, MessageSender>();
            services.AddTransient<IEmailSender, MessageSender>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseStaticFiles();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            var requestLocalizationOptions = new RequestLocalizationOptions();
            ConfigureOptions(requestLocalizationOptions);
            app.UseRequestLocalization(requestLocalizationOptions);

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Name");
            });
        }
    }

    public class HeaderFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation == null)
                return;

            switch (context.ApiDescription.ActionDescriptor)
            {
                case ControllerActionDescriptor actionDescriptor when actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() == null &&
                (actionDescriptor.MethodInfo.GetCustomAttribute<AuthorizeAttribute>() != null
                || actionDescriptor.ControllerTypeInfo.CustomAttributes.Any(e => e.AttributeType == typeof(AuthorizeAttribute))
                || actionDescriptor.ControllerTypeInfo.BaseType.CustomAttributes.Any(e => e.AttributeType == typeof(AuthorizeAttribute))):
                    operation.Parameters = operation.Parameters ?? new List<IParameter>();
                    operation.Parameters.Add(new NonBodyParameter
                    {
                        Description = "The bearer authorization header",
                        In = "header",
                        Name = "Authorization",
                        Required = true,
                        Default = "Bearer ",
                        Type = "string"
                    });
                    break;
            }
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProfileModel, User>().ForAllMembers(expression =>
            {
                expression.Condition((profile, user, value) => value != null);
            });
            CreateMap<RegisterModel, User>();
        }
    }
}