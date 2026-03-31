//Nguyên
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
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
            var builder = WebApplication.CreateBuilder(args);

            // 1. CẤU HÌNH CORS (Để FE Vite gọi được API Local)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocal", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // Port mặc định của Vite
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 2. DATABASE (Sử dụng SQL Server cho Local)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 3. CẤU HÌNH SWAGGER CHI TIẾT
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SWP-BE Local API",
                    Version = "v1.0",
                    Description = "API phục vụ dự án Gán nhãn dữ liệu (Môi trường Local)"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Dán Token vào đây (Không cần chữ Bearer):"
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
            });

            // 4. DEPENDENCY INJECTION (DI)
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
            builder.Services.AddScoped<IReviewerRepository, ReviewerRepository>();
            builder.Services.AddScoped<SuggestionService>();
            builder.Services.AddScoped<SWP_BE.Repositories.SuggestionRepository>();
            builder.Services.AddScoped<ExportService>();
            builder.Services.AddHttpContextAccessor();

            // 5. CẤU HÌNH JWT AUTH
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

            // 6. XỬ LÝ LỖI TOÀN CỤC (TRẢ VỀ JSON CHO FE DỄ ĐỌC)
            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Lỗi Server Local",
                    detail = exception?.Message
                });
            }));

            // 7. SWAGGER TRONG MÔI TRƯỜNG DEVELOPMENT
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SWP-BE Local v1.0");
                    options.RoutePrefix = string.Empty; // Mở http://localhost:PORT/ là ra Swagger luôn
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Kích hoạt CORS trước Auth
            app.UseCors("AllowLocal");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}