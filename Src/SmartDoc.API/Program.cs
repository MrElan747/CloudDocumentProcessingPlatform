using Microsoft.EntityFrameworkCore;
using SmartDoc.API.Exceptions;
using SmartDoc.Models.EntityModels;
using SmartDoc.Repository.Interfaces;
using SmartDoc.Repository.Repositories;
using SmartDoc.Services.Interfaces;
using SmartDoc.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<CloudDocumentProcessingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IServiceBusRepository, ServiceBusRepository>();
builder.Services.AddScoped<IDcoumentsService, DocumentsService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "SmartDoc API"));
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
