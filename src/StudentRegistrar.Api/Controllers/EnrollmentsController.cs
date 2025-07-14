using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;

namespace StudentRegistrar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
    {
        var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
    {
        var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentDto>> CreateEnrollment(CreateEnrollmentDto createEnrollmentDto)
    {
        var enrollment = await _enrollmentService.CreateEnrollmentAsync(createEnrollmentDto);
        return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.Id }, enrollment);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<EnrollmentDto>> UpdateEnrollmentStatus(int id, [FromBody] UpdateEnrollmentStatusDto updateDto)
    {
        var enrollment = await _enrollmentService.UpdateEnrollmentStatusAsync(id, updateDto.Status);
        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var result = await _enrollmentService.DeleteEnrollmentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

public class UpdateEnrollmentStatusDto
{
    public string Status { get; set; } = string.Empty;
}
