using FitupProject.BackgroundJobs;
using FitupProject.BLL.Commons.Securities;
using FitupProject.BLL.Commons.VNPay;
using FitupProject.BLL.Interfaces;
using FitupProject.BLL.Services;
using FitupProject.DAL.Database;
using FitupProject.DAL.Interfaces;
using FitupProject.DAL.Repositories;
using FitupProject.Filters;
using FitupProject.Middlewares;
using FitupProject.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//lấy Dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NeonDb")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FitupProject API", Version = "v1" });

    // ===== JWT Bearer on Swagger =====
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {your_jwt_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

//setup jwt
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("Missing config: Jwt:Key");

var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
            ValidIssuer = issuer,

            ValidateAudience = !string.IsNullOrWhiteSpace(audience),
            ValidAudience = audience,

            ClockSkew = TimeSpan.Zero
        };

        //json cho 401 + 403
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Unauthorized." }));
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Forbidden." }));
            }
        };
    });

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseWrapperFilter>();
});
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

//---- DI Uow + IGenericRepository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

//---- DI Background Jobs
builder.Services.AddHostedService<PaymentExpiryHostedService>();

//---- DI Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminAccountService, AdminAccountService>();
//builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IEmailSender, ResendEmailSender>();
builder.Services.AddHostedService<PendingAccountCleanupService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPTService, PTService>();
//builder.Services.AddScoped<ISlotService, SlotService>();

//---- DI Account
builder.Services.AddScoped<IAccountService, AccountService>();

//---- DI Workout flow
builder.Services.AddScoped<IWorkoutCatalogService, WorkoutCatalogService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
//---- DI Slot
builder.Services.AddScoped<ISlotService, SlotService>();
// ---- DI Booking
builder.Services.AddScoped<IBookingService, BookingService>();
//---- DI VNPAY + Conversion
builder.Services.Configure<VnPayOptions>(builder.Configuration.GetSection("VnPay"));
builder.Services.AddScoped<ITopUpService, TopUpService>();
builder.Services.AddScoped<IConversionRateService, ConversionRateService>();
// ---- DI DashBoard
builder.Services.AddScoped<IDashBoardService, DashBoardService>();

//Middleware
//builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

//app.MapGet("/", () => Results.Ok(new { name = "FitUp API", status = "running" }));
//app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();
app.Run();

