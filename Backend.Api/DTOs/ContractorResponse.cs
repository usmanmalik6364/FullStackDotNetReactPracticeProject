namespace Backend.Api.DTOs;

public class CreateContractorRequest
{
    public string ContractorNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
}