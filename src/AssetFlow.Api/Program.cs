using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Abstractions.Storage;
using AssetFlow.Application.Services;
using AssetFlow.Infrastructure.Options;
using AssetFlow.Infrastructure.Persistence;
using AssetFlow.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AssetService>();
builder.Services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();

builder.Services.Configure<BlobStorageOptions>(
    builder.Configuration.GetSection(BlobStorageOptions.SectionName));

builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

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