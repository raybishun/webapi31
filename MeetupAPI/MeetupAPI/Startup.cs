using FluentValidation;
using FluentValidation.AspNetCore;
using MeetupAPI.Authorization;
using MeetupAPI.Controllers.Filters;
using MeetupAPI.Entities;
using MeetupAPI.Identity;
using MeetupAPI.Models;
using MeetupAPI.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Text;
// using Swashbuckle.AspNetCore.Swagger;

namespace MeetupAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var jwtOptions = new JwtOptions();
            Configuration.GetSection("jwt").Bind(jwtOptions);
            services.AddSingleton(jwtOptions);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(cfg => 
            {
                cfg.RequireHttpsMetadata = false;
                cfg.TokenValidationParameters = new TokenValidationParameters
                { 
                    ValidIssuer = jwtOptions.JwtIssuer,
                    ValidAudience = jwtOptions.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.JwtKey))
                };
            });

            services.AddAuthorization(options => {
                options.AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality", "German", "English"));
                options.AddPolicy("AtLeast18", builder => builder.AddRequirements(new MinimumAgeRequirement(18)));
            });

            services.AddScoped<TimeTrackFilter>();
            services.AddScoped<IAuthorizationHandler, MeetupResourceOperationHandler>();
            services.AddScoped<IAuthorizationHandler, MinimumAgeHandler>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddControllers(options => options.Filters.Add(typeof(ExceptionFilter))).AddFluentValidation();
            services.AddScoped<IValidator<RegisterUserDto>, RegisterUserValidator>();
            services.AddScoped<IValidator<MeetupQuery>, MeetupQueryValidator>();
            // services.AddDbContext<MeetupContext>();
            services.AddDbContext<MeetupContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<MeetupSeeder>();
            services.AddAutoMapper(this.GetType().Assembly);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "MeetupAPI", Version = "v1"});
            });
            services.AddCors(options => {
                options.AddPolicy("FrontEndClient", builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:5001"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MeetupSeeder meetupSeeder, MeetupContext context)
        {
            RunMigrations(context);

            app.UseResponseCaching();
            app.UseStaticFiles();
            app.UseCors("FrontEndClient");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeetupAPI v1");
            });
                        
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            // rb demo
            // app.Use(async (context, next) => { await context.Response.WriteAsync("Hello World!"); });

            // app.UseMvc();

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            meetupSeeder.Seed();
        }

        /// <summary>
        /// Automatically run migrations when needed.
        /// </summary>
        /// <param name="context"></param>
        private void RunMigrations(MeetupContext context)
        {
            var pendingMigrations = context.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                context.Database.Migrate();
            }
        }
    }
}
