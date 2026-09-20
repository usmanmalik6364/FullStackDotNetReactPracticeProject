using Backend.Api.Data;
using Backend.Api.DTOs;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services;

public class ContractorService : IContractorService
{
    private readonly ApplicationDbContext _db;

    public ContractorService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ContractorResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var contractor = await _db.Contractors
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (contractor is null)
        {
            return null;
        }

        return new ContractorResponse
        {
            Id = contractor.Id,
            ContractorNumber = contractor.ContractorNumber,
            Name = contractor.Name,
            CompanyName = contractor.CompanyName,
            IsActive = contractor.IsActive,
            CreatedAtUtc = contractor.CreatedAtUtc,
            RowVersion = Convert.ToBase64String(contractor.RowVersion)
        };
    }

    public async Task<ContractorResponse> CreateAsync(
    CreateContractorRequest request,
    CancellationToken cancellationToken)
    {
        var contractor = new Contractor
        {
            Id = Guid.NewGuid(),
            ContractorNumber = request.ContractorNumber,
            Name = request.Name,
            CompanyName = request.CompanyName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Contractors.Add(contractor);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 ||
                   sqlException.Number == 2627))
        {
            throw new DuplicateResourceException(
                $"A contractor with number '{request.ContractorNumber}' already exists.");
        }

        return new ContractorResponse
        {
            Id = contractor.Id,
            ContractorNumber = contractor.ContractorNumber,
            Name = contractor.Name,
            CompanyName = contractor.CompanyName,
            IsActive = contractor.IsActive,
            CreatedAtUtc = contractor.CreatedAtUtc,
            RowVersion = Convert.ToBase64String(contractor.RowVersion)
        };
    }

    public async Task<ContractorResponse?> UpdateAsync(
    Guid id,
    UpdateContractorRequest request,
    CancellationToken cancellationToken)
    {
        var contractor = await _db.Contractors
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (contractor is null)
        {
            return null;
        }

        var originalRowVersion =
            Convert.FromBase64String(request.RowVersion);

        _db.Entry(contractor)
            .Property(x => x.RowVersion)
            .OriginalValue = originalRowVersion;

        contractor.Name = request.Name;
        contractor.CompanyName = request.CompanyName;
        contractor.IsActive = request.IsActive;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException(
                "The contractor was modified by another request. Reload the resource and try again.");
        }

        return new ContractorResponse
        {
            Id = contractor.Id,
            ContractorNumber = contractor.ContractorNumber,
            Name = contractor.Name,
            CompanyName = contractor.CompanyName,
            IsActive = contractor.IsActive,
            CreatedAtUtc = contractor.CreatedAtUtc,
            RowVersion = Convert.ToBase64String(contractor.RowVersion)
        };
    }


}