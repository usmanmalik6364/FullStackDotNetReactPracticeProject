using Backend.Api.Caching;
using Backend.Api.Data;
using Backend.Api.DTOs;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
namespace Backend.Api.Services;

public class ContractorService : IContractorService
{
    private readonly ApplicationDbContext _db;
    private readonly ICacheService _cache;
    private readonly CacheLockProvider _cacheLocks;
    private readonly IDistributedLockService _distributedLock;

    public ContractorService(
    ApplicationDbContext db,
    ICacheService cache,
    CacheLockProvider cacheLocks,
    IDistributedLockService distributedLock)
    {
        _db = db;
        _cache = cache;
        _cacheLocks = cacheLocks;
        _distributedLock = distributedLock;
    }

    public async Task<ContractorResponse?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken)
    {
        var cacheKey = $"contractor:{id}";

        var cached =
            await _cache.GetAsync<ContractorResponse>(
                cacheKey,
                cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var lockKey = $"lock:contractor:{id}";
        var lockToken = Guid.NewGuid().ToString();

        var acquired =
            await _distributedLock.AcquireAsync(
                lockKey,
                lockToken,
                TimeSpan.FromSeconds(5));

        if (!acquired)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(100),
                    cancellationToken);

                var cachedResult =
                    await _cache.GetAsync<ContractorResponse>(
                        cacheKey,
                        cancellationToken);

                if (cachedResult is not null)
                {
                    return cachedResult;
                }
            }
        }

        try
        {
            cached =
                await _cache.GetAsync<ContractorResponse>(
                    cacheKey,
                    cancellationToken);

            if (cached is not null)
            {
                return cached;
            }

            var contractor = await _db.Contractors
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ContractorResponse
                {
                    Id = x.Id,
                    ContractorNumber = x.ContractorNumber,
                    Name = x.Name,
                    CompanyName = x.CompanyName,
                    IsActive = x.IsActive,
                    CreatedAtUtc = x.CreatedAtUtc,
                    RowVersion = Convert.ToBase64String(
                        x.RowVersion)
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

            if (contractor is null)
            {
                return null;
            }

            await _cache.SetAsync(
                cacheKey,
                contractor,
                TimeSpan.FromMinutes(5),
                cancellationToken);

            return contractor;
        }
        finally
        {
            await _distributedLock.ReleaseAsync(
                lockKey,
                lockToken);
        }
    }
    public async Task<ContractorResponse> CreateAsync(
    CreateContractorRequest request,
    CancellationToken cancellationToken)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        try
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

            await _db.SaveChangesAsync(cancellationToken);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "Contractor",
                EntityId = contractor.Id.ToString(),
                Action = "Created",
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.AuditLogs.Add(auditLog);

            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

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
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 ||
                   sqlException.Number == 2627))
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new DuplicateResourceException(
                $"A contractor with number '{request.ContractorNumber}' already exists.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
            var cacheKey = $"contractor:{id}";

            await _cache.RemoveAsync(
                        $"contractor:{id}",
                        cancellationToken);
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
    public async Task<IReadOnlyList<ContractorResponse>> GetByCompanyAsync(
    string companyName,
    CancellationToken cancellationToken)
    {
        return await _db.Contractors
            .AsNoTracking()
            .Where(x => x.CompanyName == companyName)
            .Select(x => new ContractorResponse
            {
                Id = x.Id,
                ContractorNumber = x.ContractorNumber,
                Name = x.Name,
                CompanyName = x.CompanyName,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                RowVersion = Convert.ToBase64String(x.RowVersion)
            })
            .ToListAsync(cancellationToken);
    }


}