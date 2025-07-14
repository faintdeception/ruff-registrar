using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;

namespace StudentRegistrar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GradeRecordDto>>> GetGrades()
    {
        var grades = await _gradeService.GetAllGradesAsync();
        return Ok(grades);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GradeRecordDto>> GetGrade(int id)
    {
        var grade = await _gradeService.GetGradeByIdAsync(id);
        if (grade == null)
            return NotFound();

        return Ok(grade);
    }

    [HttpPost]
    public async Task<ActionResult<GradeRecordDto>> CreateGrade(CreateGradeRecordDto createGradeDto)
    {
        var grade = await _gradeService.CreateGradeAsync(createGradeDto);
        return CreatedAtAction(nameof(GetGrade), new { id = grade.Id }, grade);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GradeRecordDto>> UpdateGrade(int id, CreateGradeRecordDto updateGradeDto)
    {
        var grade = await _gradeService.UpdateGradeAsync(id, updateGradeDto);
        if (grade == null)
            return NotFound();

        return Ok(grade);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGrade(int id)
    {
        var result = await _gradeService.DeleteGradeAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
