using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
            // BẮT BUỘC ĐỂ TRỊ LỖI LỆCH MÚI GIỜ CỦA POSTGRESQL (SUPABASE)
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

            // ===== DB =====
            // Đã đổi sang UseNpgsql để dùng PostgreSQL của Supabase
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ===== Controllers & Swagger =====
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // CẤU HÌNH SWAGGER ĐỂ HIỆN GHI CHÚ VÀ NÚT AUTHORIZE
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Data Labeling Support API",
                    Version = "v1.0",
                    Description = "API phục vụ dự án Gán nhãn dữ liệu - Nhóm 5 Topic 4"
                });

                // Cấu hình nút Authorize (Ổ khóa)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Dán Token vào đây (Không cần gõ chữ Bearer):"
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

                // QUAN TRỌNG: Summary tiếng Việt từ Controller
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // ===== Dependency Injection (DI) ===== (ĐÃ DỌN SẠCH CÁC DÒNG BỊ TRÙNG)
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

            // 2. TRẢ VỀ JSON LỖI (Giúp FE dễ xử lý)
            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var responseObj = new
                {
                    message = "Yêu cầu không hợp lệ hoặc bạn không có quyền truy cập",
                    error = context.HttpContext.Response.StatusCode switch
                    {
                        401 => "Unauthorized",
                        403 => "Forbidden",
                        404 => "Not Found",
                        405 => "Method Not Allowed",
                        _ => "Error"
                    },
                    statusCode = context.HttpContext.Response.StatusCode
                };
                await context.HttpContext.Response.WriteAsJsonAsync(responseObj);
            });

            // ===== Swagger Middleware =====
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SWP-BE v1.0");
                options.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}