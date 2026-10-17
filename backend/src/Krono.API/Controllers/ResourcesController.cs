using Krono.Application.DTOs;
using Krono.Domain.Entities;
using Krono.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Krono.API.Controllers;

[ApiController]
[Route("api/v1/operator-skills")]
public class OperatorSkillsController : ControllerBase
{
    private readonly IOperatorSkillRepository _repository;

    public OperatorSkillsController(IOperatorSkillRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<OperatorSkillDto>>> GetAll()
    {
        var skills = await _repository.GetAllAsync();
        return Ok(skills.Select(s => new OperatorSkillDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description
        }));
    }

    [HttpPost]
    public async Task<ActionResult<OperatorSkillDto>> Create([FromBody] CreateOperatorSkillRequest request)
    {
        var skill = new OperatorSkill
        {
            Name = request.Name,
            Description = request.Description
        };
        await _repository.AddAsync(skill);
        return CreatedAtAction(nameof(GetAll), new { id = skill.Id }, new OperatorSkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/machine-types")]
public class MachineTypesController : ControllerBase
{
    private readonly IMachineTypeRepository _repository;

    public MachineTypesController(IMachineTypeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<MachineTypeDto>>> GetAll()
    {
        var machines = await _repository.GetAllAsync();
        return Ok(machines.Select(m => new MachineTypeDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            TotalAvailable = m.TotalAvailable
        }));
    }

    [HttpPost]
    public async Task<ActionResult<MachineTypeDto>> Create([FromBody] CreateMachineTypeRequest request)
    {
        var machine = new MachineType
        {
            Name = request.Name,
            Description = request.Description,
            TotalAvailable = request.TotalAvailable
        };
        await _repository.AddAsync(machine);
        return CreatedAtAction(nameof(GetAll), new { id = machine.Id }, new MachineTypeDto
        {
            Id = machine.Id,
            Name = machine.Name,
            Description = machine.Description,
            TotalAvailable = machine.TotalAvailable
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
