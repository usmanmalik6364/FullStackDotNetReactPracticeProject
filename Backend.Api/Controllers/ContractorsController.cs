using Backend.Api.DTOs;
using Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractorsController : ControllerBase
{
    private readonly IContractorService _service;

    public ContractorsController(IContractorService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContractorResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var contractor = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (contractor is null)
        {
            return NotFound();
        }

        return Ok(contractor);
    }

    [HttpPost]
    public async Task<ActionResult<ContractorResponse>> Create(
        CreateContractorRequest request,
        CancellationToken cancellationToken)
    {
        var contractor = await _service.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = contractor.Id },
            contractor);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContractorResponse>> Update(
    Guid id,
    UpdateContractorRequest request,
    CancellationToken cancellationToken)
    {
        var contractor = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (contractor is null)
        {
            return NotFound();
        }

        return Ok(contractor);
    }
}