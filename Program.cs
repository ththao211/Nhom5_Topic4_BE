using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Quan trọng
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWP_BE.Data;
using SWP_BE.Repositories;
using SWP_BE.Services;

namespace SWP_BE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Cấu hình CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ===== DB Connection (ĐÃ ĐỔI SANG SQL SERVER) =====
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ===== Swagger Configuration =====
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Data Labeling Support API",
                    Version = "v1.0",
                    Description = "API phục vụ dự án Gán nhãn dữ liệu - SQL Server Local"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Dán Token vào đây:"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
            });

            // ===== Dependency Injection (DI) =====
            builder.Services.AddScoped<ILabelRepository, LabelRepository>();
            builder.Services.AddScoped<ILabelService, LabelService>();
            builder.Services.AddScoped<IProjectLabelRepository, ProjectLabelRepository>();
            builder.Services.AddScoped<IProjectLabelService, ProjectLabelService>();
            builder.Services.AddScoped<ILabelingTaskRepository, LabelingTaskRepository>();
            builder.Services.AddScoped<ILabelingTaskService, LabelingTaskService>();
            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAnnotatorRepository, AnnotatorRepository>();
            builder.Services.AddScoped<AnnotatorService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IReputationRepository, ReputationRepository>();
            builder.Services.AddScoped<ReputationService>();
            builder.Services.AddScoped<IProgressService, ProgressService>();

            // ===== JWT AUTH =====
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });
            var app = builder.Build();

            // Cấu hình Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SWP-BE v1.0");
                    options.RoutePrefix = string.Empty;
                });
            }

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var code = context.HttpContext.Response.StatusCode;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    message = "Lỗi hệ thống hoặc không có quyền",
                    statusCode = code
                });
            });

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}