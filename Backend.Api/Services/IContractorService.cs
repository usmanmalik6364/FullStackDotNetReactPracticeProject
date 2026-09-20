using Backend.Api.DTOs;

namespace Backend.Api.Services;

public interface IContractorService
{
    Task<ContractorResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ContractorResponse> CreateAsync(
        CreateContractorRequest request,
        CancellationToken cancellationToken);

    Task<ContractorResponse?> UpdateAsync(
    Guid id,
    UpdateContractorRequest request,
    CancellationToken cancellationToken);
    Task<IReadOnlyList<ContractorResponse>> GetByCompanyAsync(
    string companyName,
    CancellationToken cancellationToken);
}