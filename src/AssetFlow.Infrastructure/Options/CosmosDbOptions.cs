namespace AssetFlow.Infrastructure.Options
{
    public class CosmosDbOptions
    {
        public const string SectionName = "CosmosDb";
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = "AssetFlowDb";
        public string ContainerName { get; set; } = "Assets";
    }
}
