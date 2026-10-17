using Krono.Application.DTOs;
using Krono.Domain.Entities;
using Krono.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Krono.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(MapToDto(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Code = request.Code,
            Name = request.Name,
            Category = request.Category,
            Stages = request.Stages.Select(s => new ProductionStage
            {
                Name = s.Name,
                SequenceOrder = s.SequenceOrder,
                MachineTypeId = s.MachineTypeId,
                MachineQuantity = s.MachineQuantity,
                OperatorSkillId = s.OperatorSkillId,
                DefaultOperators = s.DefaultOperators,
                LaborMode = s.LaborMode,
                SetupFixedMinutes = s.SetupFixedMinutes,
                SetupMultiplierMinutes = s.SetupMultiplierMinutes,
                ProdFixedMinutes = s.ProdFixedMinutes,
                ProdMultiplierMinutes = s.ProdMultiplierMinutes,
                PredecessorStageIds = s.PredecessorStageIds
            }).ToList()
        };

        await _repository.AddAsync(product);
        var created = await _repository.GetByIdAsync(product.Id);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, MapToDto(created!));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return NotFound();

        product.Code = request.Code;
        product.Name = request.Name;
        product.Category = request.Category;
        product.Stages.Clear();

        foreach (var s in request.Stages)
        {
            product.Stages.Add(new ProductionStage
            {
                ProductId = product.Id,
                Name = s.Name,
                SequenceOrder = s.SequenceOrder,
                MachineTypeId = s.MachineTypeId,
                MachineQuantity = s.MachineQuantity,
                OperatorSkillId = s.OperatorSkillId,
                DefaultOperators = s.DefaultOperators,
                LaborMode = s.LaborMode,
                SetupFixedMinutes = s.SetupFixedMinutes,
                SetupMultiplierMinutes = s.SetupMultiplierMinutes,
                ProdFixedMinutes = s.ProdFixedMinutes,
                ProdMultiplierMinutes = s.ProdMultiplierMinutes,
                PredecessorStageIds = s.PredecessorStageIds
            });
        }

        await _repository.UpdateAsync(product);
        var updated = await _repository.GetByIdAsync(id);
        return Ok(MapToDto(updated!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Category = p.Category,
            Stages = p.Stages.Select(s => new ProductionStageDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                Name = s.Name,
                SequenceOrder = s.SequenceOrder,
                MachineTypeId = s.MachineTypeId,
                MachineTypeName = s.MachineType?.Name,
                MachineQuantity = s.MachineQuantity,
                OperatorSkillId = s.OperatorSkillId,
                OperatorSkillName = s.OperatorSkill?.Name,
                DefaultOperators = s.DefaultOperators,
                LaborMode = s.LaborMode,
                SetupFixedMinutes = s.SetupFixedMinutes,
                SetupMultiplierMinutes = s.SetupMultiplierMinutes,
                ProdFixedMinutes = s.ProdFixedMinutes,
                ProdMultiplierMinutes = s.ProdMultiplierMinutes,
                PredecessorStageIds = s.PredecessorStageIds
            }).OrderBy(s => s.SequenceOrder).ToList()
        };
    }
}
