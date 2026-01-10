using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Globalization;
using System.Text;
using System.Text.Json;
using TechsysLog.Application;
using TechsysLog.Application.Common;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Data;
using TechsysLog.Infrastructure.Data.Context;
using TechsysLog.Infrastructure.Data.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

            var result = BusinessResult<object>.Failure("You are not authorized. Please log in again");

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var result = BusinessResult<object>.Failure("You do not have permission to access this resource.");
            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    };
});

var app = builder.Build();


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
                await context.Response.WriteAsJsonAsync(BusinessResult<object>.Failure("An unexpected error occurred."));

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
