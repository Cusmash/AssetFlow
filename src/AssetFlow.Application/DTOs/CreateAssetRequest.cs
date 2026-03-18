using System;

public class CreateAssetRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string ContentType { get; set; } = default!;
}
