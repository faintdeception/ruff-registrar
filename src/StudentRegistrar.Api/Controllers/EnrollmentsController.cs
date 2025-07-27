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
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(Guid id)
    {
        var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpGet("legacy/{id:int}")]
    [Obsolete("Use GetEnrollment(Guid id) instead")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollmentLegacy(int id)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
        #pragma warning restore CS0618 // Type or member is obsolete
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
    public async Task<ActionResult<EnrollmentDto>> UpdateEnrollmentStatus(Guid id, [FromBody] UpdateEnrollmentStatusDto updateDto)
    {
        var enrollment = await _enrollmentService.UpdateEnrollmentStatusAsync(id, updateDto.Status);
        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpPut("legacy/{id:int}/status")]
    [Obsolete("Use UpdateEnrollmentStatus(Guid id, UpdateEnrollmentStatusDto) instead")]
    public async Task<ActionResult<EnrollmentDto>> UpdateEnrollmentStatusLegacy(int id, [FromBody] UpdateEnrollmentStatusDto updateDto)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var enrollment = await _enrollmentService.UpdateEnrollmentStatusAsync(id, updateDto.Status);
        #pragma warning restore CS0618 // Type or member is obsolete
        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(Guid id)
    {
        var result = await _enrollmentService.DeleteEnrollmentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpDelete("legacy/{id:int}")]
    [Obsolete("Use DeleteEnrollment(Guid id) instead")]
    public async Task<IActionResult> DeleteEnrollmentLegacy(int id)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var result = await _enrollmentService.DeleteEnrollmentAsync(id);
        #pragma warning restore CS0618 // Type or member is obsolete
        if (!result)
            return NotFound();

        return NoContent();
    }

    // New endpoints for enhanced functionality
    [HttpGet("by-student/{studentId}")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsByStudent(Guid studentId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(studentId);
        return Ok(enrollments);
    }

    [HttpGet("by-course/{courseId}")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsByCourse(Guid courseId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
        return Ok(enrollments);
    }

    [HttpGet("by-semester/{semesterId}")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsBySemester(Guid semesterId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsBySemesterAsync(semesterId);
        return Ok(enrollments);
    }
}

public class UpdateEnrollmentStatusDto
{
    public string Status { get; set; } = string.Empty;
}
