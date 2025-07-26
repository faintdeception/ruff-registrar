using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;

namespace StudentRegistrar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewCoursesController : ControllerBase
{
    private readonly INewCourseService _courseService;
    private readonly ILogger<NewCoursesController> _logger;

    public NewCoursesController(
        INewCourseService courseService,
        ILogger<NewCoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewCourseDto>>> GetCourses([FromQuery] Guid? semesterId = null)
    {
        try
        {
            var courses = semesterId.HasValue 
                ? await _courseService.GetCoursesBySemesterAsync(semesterId.Value)
                : await _courseService.GetAllCoursesAsync();
            
            return Ok(courses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NewCourseDto>> GetCourse(Guid id)
    {
        try
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            return Ok(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<NewCourseDto>> CreateCourse(CreateNewCourseDto createDto)
    {
        try
        {
            var course = await _courseService.CreateCourseAsync(createDto);
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<NewCourseDto>> UpdateCourse(Guid id, UpdateNewCourseDto updateDto)
    {
        try
        {
            var course = await _courseService.UpdateCourseAsync(id, updateDto);
            if (course == null)
                return NotFound();

            return Ok(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> DeleteCourse(Guid id)
    {
        try
        {
            var deleted = await _courseService.DeleteCourseAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
