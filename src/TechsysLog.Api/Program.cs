using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
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

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
