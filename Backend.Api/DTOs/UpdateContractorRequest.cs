namespace Backend.Api.DTOs;

public class UpdateContractorRequest
{
    public string Name { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string RowVersion { get; set; } = string.Empty;
}