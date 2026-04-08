using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Abstractions.Storage;
using AssetFlow.Application.Services;
using AssetFlow.Infrastructure.Options;
using AssetFlow.Infrastructure.Persistence;
using AssetFlow.Infrastructure.Storage;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];

if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AssetService>();
//builder.Services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();

builder.Services.Configure<CosmosDbOptions>(
    builder.Configuration.GetSection(CosmosDbOptions.SectionName));

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<
        IOptions<CosmosDbOptions>>().Value;

    return new CosmosClient(options.ConnectionString);
});

builder.Services.AddSingleton<IAssetRepository, CosmosAssetRepository>();

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