using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Sammaron.Authentication;
using Sammaron.Authentication.JwtBearer;
using Sammaron.Authentication.JwtBearer.Extensions;
using Sammaron.Authentication.JwtBearer.Handlers;
using Sammaron.Core.Data;
using Sammaron.Core.Interfaces;
using Sammaron.Core.Models;
using Sammaron.Core.Services;
using Sammaron.Server.Extensions;
using Sammaron.Server.ViewModels;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
            services.AddDbContext<ApiContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(ConfigureOptions);
            services.AddIdentityBase<User, IdentityRole>(options =>
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

            services.AddMvc();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Sammaorn Jwt Authentication Server",
                    Description = "A server that uses ASP.NET Identity Entity Framework as a persistence store and Bearer for user authentication.",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "Rahma Ahmed",
                        Email = "rahmed.sammaron@gmail.com",
                        Url = "https://github.com/rahmadSammaron"
                    },
                    License = new License
                    {
                        Name = "MIT License",
                        Url = "https://github.com/rahmadSammaron/Ionic-3-Visual-Studio-Template/blob/master/LICENSE.md"
                    }
                });
                options.OperationFilter<HeaderFilter>();
                options.DescribeAllParametersInCamelCase();
                options.DescribeAllEnumsAsStrings();
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
                options.AddScheme(JwtBearerDefaults.AuthenticationScheme, builder =>
                {
                    builder.DisplayName = "Jwt Bearer";
                    builder.HandlerType = typeof(BearerAuthenticationHandler);
                });
            });

            services.Configure<BearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Audience = "*";
                options.ClaimsIssuer = IdentityConstants.ApplicationScheme;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("SigningKey"))),
                    TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("DecryptionKey"))),
                    ValidIssuers = new[] { IdentityConstants.ApplicationScheme },
                    ValidAudiences = new[] { "*" },
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new BearerEvents();
            });

            services.AddRouting(e => e.LowercaseUrls = true);
            services.AddAutoMapper(expression => { expression.AddProfile<MappingProfile>(); });
            services.AddTransient<ISmsSender, MessageSender>();
            services.AddTransient<IEmailSender, MessageSender>();
            services.Replace(ServiceDescriptor.Scoped<IAuthenticationService, BearerAuthenticationService>());
            services.AddScoped<IBearerAuthenticationService, BearerAuthenticationService>();
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
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sammaorn OAuth.2 Server"); });
        }
    }

    public class HeaderFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation == null)
                return;

            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor &&
                !actionDescriptor.MethodInfo.HasAttribute<AllowAnonymousAttribute>() &&
                actionDescriptor.HasAttribute<AuthorizeAttribute>())
            {
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
            }
        }
    }

    public class LowercaseDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;
            swaggerDoc.Paths = new Dictionary<string, PathItem>();

            foreach (var path in paths)
            {
                var parts = path.Key.Split("/").Where(e => !string.IsNullOrEmpty(e)).ToList();
                for (var index = 0; index < parts.Count; index++)
                {
                    parts[index] = parts[index].ToCamelCase();
                }
                swaggerDoc.Paths.Add(string.Join("/", parts), path.Value);
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