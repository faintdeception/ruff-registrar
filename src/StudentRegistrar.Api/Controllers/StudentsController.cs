using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;

namespace StudentRegistrar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents()
    {
        var students = await _studentService.GetAllStudentsAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetStudent(Guid id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null)
            return NotFound();

        return Ok(student);
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpGet("legacy/{id:int}")]
    [Obsolete("Use GetStudent(Guid id) instead")]
    public async Task<ActionResult<StudentDto>> GetStudentLegacy(int id)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var student = await _studentService.GetStudentByIdAsync(id);
        #pragma warning restore CS0618 // Type or member is obsolete
        if (student == null)
            return NotFound();

        return Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentDto createStudentDto)
    {
        var student = await _studentService.CreateStudentAsync(createStudentDto);
        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentDto>> UpdateStudent(Guid id, UpdateStudentDto updateStudentDto)
    {
        var student = await _studentService.UpdateStudentAsync(id, updateStudentDto);
        if (student == null)
            return NotFound();

        return Ok(student);
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpPut("legacy/{id:int}")]
    [Obsolete("Use UpdateStudent(Guid id, UpdateStudentDto) instead")]
    public async Task<ActionResult<StudentDto>> UpdateStudentLegacy(int id, UpdateStudentDto updateStudentDto)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var student = await _studentService.UpdateStudentAsync(id, updateStudentDto);
        #pragma warning restore CS0618 // Type or member is obsolete
        if (student == null)
            return NotFound();

        return Ok(student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var result = await _studentService.DeleteStudentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpDelete("legacy/{id:int}")]
    [Obsolete("Use DeleteStudent(Guid id) instead")]
    public async Task<IActionResult> DeleteStudentLegacy(int id)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var result = await _studentService.DeleteStudentAsync(id);
        #pragma warning restore CS0618 // Type or member is obsolete
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/enrollments")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetStudentEnrollments(Guid id)
    {
        var enrollments = await _studentService.GetStudentEnrollmentsAsync(id);
        return Ok(enrollments);
    }

    // Legacy support for integer IDs (will be deprecated)
    [HttpGet("legacy/{id:int}/enrollments")]
    [Obsolete("Use GetStudentEnrollments(Guid id) instead")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetStudentEnrollmentsLegacy(int id)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var enrollments = await _studentService.GetStudentEnrollmentsAsync(id);
        #pragma warning restore CS0618 // Type or member is obsolete
        return Ok(enrollments);
    }

    // New endpoint for account holder's students
    [HttpGet("by-account/{accountHolderId}")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByAccountHolder(Guid accountHolderId)
    {
        var students = await _studentService.GetStudentsByAccountHolderAsync(accountHolderId);
        return Ok(students);
    }
}
