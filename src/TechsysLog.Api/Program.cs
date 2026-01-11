using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Globalization;
using System.Text;
using System.Text.Json;
using TechsysLog.Application;
using TechsysLog.Application.Common;
using TechsysLog.Application.Hubs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Seed;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Data;
using TechsysLog.Infrastructure.Data.Context;
using TechsysLog.Infrastructure.Data.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TechsysLog API",
        Version = "v1"
    });

    // Configuração do Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta forma: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb"))
);

builder.Services.AddDbContext<TechsysLogMongoDbContext>((sp, options) =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    options.UseMongoDB(mongoClient, "TechsysLogDB");
});

builder.Services.AddHttpClient();

//repositories
builder.Services.AddInfrastructure(builder.Configuration);

//services 
builder.Services.AddApplicationServices(builder.Configuration);

// Configure CORS para SignalR 
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000") 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddDataAnnotationsLocalization()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var errorResponse = BusinessResult<object>.Failure(errors);

            return new BadRequestObjectResult(errorResponse);
        };
    });


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var configKey = builder.Configuration["Jwt:Key"];
var key = Encoding.ASCII.GetBytes(configKey ?? "");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,

        //ClockSkew = TimeSpan.Zero
    };

    x.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = BusinessResult<object>.Failure("Você não está autorizado. Por favor, faça login novamente.");

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var result = BusinessResult<object>.Failure("Você não tem permissão para acessar este recurso");
            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    };
});


//SignalR 
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 102400000; // 100MB
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

var app = builder.Build();


//seed gera registros inicial
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    await DbSeeder.SeedAdminAsync(userService, userRepository);
}

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            var ex = exceptionHandlerPathFeature.Error;

            if (ex is BusinessException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(BusinessResult<object>.Failure(ex.Message));
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(BusinessResult<object>.Failure("Ocorreu um erro inesperado."));

                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Unhandled exception occurred for request {Path}", context.Request.Path);
            }
        }
    });
});

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("pt-BR")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");


app.Run();
