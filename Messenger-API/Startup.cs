using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Messenger_API.Authentication;
using Messenger_API.Data;
using Messenger_API.Hubs;
using Messenger_API.Services;
using Messenger_API.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Messenger_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Constants = new Constants(configuration);
        }

        public static Constants Constants { get; private set; }

        readonly string AllowAllSpecificOrigins = "_allowAllSpecificOrigins"; // used at CORS problem


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowAllSpecificOrigins, builder => 
                {
                    builder
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST", "OPTIONS")
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials();
                });
            });

            services.AddSignalR();
            services.AddControllers();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<ITokenRepository, UserRepository>();

            services.AddTransient<ITokenHandler, RefreshTokenHandler>();
            services.AddTransient<IUserRepository, UserRepository>();

            services.AddDbContext<MessageContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("MessageContextConnection"));
            });

            //For Entity Framework
            services.AddDbContext<UserContext>(options => options.UseSqlServer(Configuration.GetConnectionString("UserContextConnection")));
            //For Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<UserContext>()
                .AddDefaultTokenProviders();

            //Adding Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            //Adding Jwt Bearer    
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:ApiKey"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            var list = accessToken.ToString().Split(" ");
                            if (list.Length > 1)
                            {
                                context.Token = list[1];
                            }
                            else
                            {
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });         
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(AllowAllSpecificOrigins);

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/messagehub");
            });
        }
    }
}
