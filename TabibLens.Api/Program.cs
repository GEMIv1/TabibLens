using Application.DTOs;
using Application.Services.Abstraction;
using Application.Services.Implementation;
using Domain.Interfaces;
using Infra.Data;
using Infra.ExternalApis;
using Infra.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TabibLens.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
            builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
            builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();

            builder.Services.AddExternalApis(builder.Configuration);

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            var app = builder.Build();

            app.UseMiddleware<TabibLens.Api.Middleware.ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Run automatic database migrations on startup
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            app.Run();
        }
    }
}
