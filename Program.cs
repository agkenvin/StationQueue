using Microsoft.Extensions.Caching.Memory;
using StationQueue.Data;
using StationQueue.Models;
using StationQueue.Services;
using StationQueue.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add developer defined services
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDatabaseSettings"));
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddMemoryCache();
builder.Services.AddLogging();

builder.Services.AddScoped<ISongService, SongService>(); 
builder.Services.AddScoped<IQueueService, QueueService>(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
