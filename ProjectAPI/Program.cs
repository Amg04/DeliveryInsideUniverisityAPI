using BAL.interfaces;
using BLLProject.Repositories;
using DAL.Data;
using DAL.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using Utilities;

namespace ProjectAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            //-----------------

            builder.Services.AddDistributedMemoryCache(); // ≈÷«›… ﬂ«‘ ›Ì «·–«ﬂ—…
            builder.Services.AddSession(); //  ›⁄Ì· «·Ã·”« 
            builder.Services.AddHttpContextAccessor(); //  ›⁄Ì· HttpContext


            builder.Services.AddDbContext<RestaurantAPIContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("CS"));
            });

            // Stripe
            builder.Services.Configure<Stripedata>(builder.Configuration.GetSection("stripe"));
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

            builder.Services.AddIdentity<User, IdentityRole>()
              .AddEntityFrameworkStores<RestaurantAPIContext>()
              .AddDefaultTokenProviders();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddAuthentication(Options =>
            {
                // Check jwt header
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // Account/Login  => by default ⁄‘«‰ „ÌÊœﬂ«‘ ⁄·Ì
                // [Authorize] return Unauthorized »œ·  not found
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // [Authorize] => Jwt «Ì Õ«Ã… „⁄·Â«‘ œÌ ÂÌ‘ €· ⁄·Ì 
                // Jwt ÂÌ»’ ⁄·Ì  cookie  »œ· „« Ì»’ ⁄·Ì «·
                Options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(Options =>
            {
                // verified key Õÿ Â‰« «·ﬁÌÊœ «··Ì «‰  ⁄«Ê“Â«
                // ÂœÌ·Ê «·ﬂÌ Ê «· Êﬂ‰ ·Ê ÿ·⁄ ‰›” «·œ« « Ì»ﬁ« ’Õ
                Options.SaveToken = true;
                Options.RequireHttpsMetadata = false; 
                Options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:IssuerIP"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:AudienceIP"], 
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(builder.Configuration["JWT:SecretKey"])) 
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            #region Swagger Setting

            // Bearer ·«“„ Ì»œ« »  Authorization «·‘ﬂ· «··Ì Â ⁄„· »Ì… 
            //Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
            //    eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9y
            //    Zy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltc
            //    y9uYW1laWRlbnRpZmllciI6ImE3ZmU1NmRjLT
            //    g5NTMtNDAzZS1hNjQyLWUxNWU4Yjg2OTI1ZCIs
            //    Imh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3d
            //    zLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbW
            //    UiOiJBaG1lZCIsImp0aSI6ImIyYmU0OTZkLTM1OT
            //    ItNDUzMi04ZTEyLWY3OTM3NTczYzk5MCIsImV4cCI6MTc
            //    0Mjk5MDg5MCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MTkxL
            //    yIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDIwMC8ifQ.6qz1p
            //    I8qX - 3we - 7q9wqiDpvCI7ZtKE65hRR0cGSxoyk


            builder.Services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation????
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = /*"v1"*/ConfigurationManager.Instance.GetSetting("AppName"),
                    Title =/* "Delivery API "*/ConfigurationManager.Instance.GetSetting("Version"),
                    Description = "API FOR SW2"
                });
                //?To Enable authorization using Swagger(JWT)
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your?valid token in the text input below.Example:Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                    {
                    Reference = new OpenApiReference
                    {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                    }
                    },
                    new string[] {}
                    }
                    });
            });

            #endregion

            //------------------

            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

           

            var app = builder.Build();

            #region Updaet Database

            //save code
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var dbContext = services.GetRequiredService<RestaurantAPIContext>();
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "an error accured during apply migrations");
                }
            }

            #endregion

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:Secretkey").Get<string>();

            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
           

            app.MapControllers();

            app.Run();
        }
    }
}
