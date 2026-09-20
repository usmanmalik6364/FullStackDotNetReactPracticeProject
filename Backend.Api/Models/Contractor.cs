namespace Backend.Api.Models;

public class Contractor
{
    public Guid Id { get; set; }

    public string ContractorNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}