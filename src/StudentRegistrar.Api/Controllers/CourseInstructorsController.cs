using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using System.IdentityModel.Tokens.Jwt;

namespace StudentRegistrar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CourseInstructorsController : ControllerBase
{
    private readonly ICourseInstructorService _courseInstructorService;
    private readonly ILogger<CourseInstructorsController> _logger;

    public CourseInstructorsController(
        ICourseInstructorService courseInstructorService,
        ILogger<CourseInstructorsController> logger)
    {
        _courseInstructorService = courseInstructorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseInstructorDto>>> GetCourseInstructors()
    {
        try
        {
            // Allow all authenticated users to view course instructors
            var instructors = await _courseInstructorService.GetAllCourseInstructorsAsync();
            return Ok(instructors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course instructors");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseInstructorDto>> GetCourseInstructor(Guid id)
    {
        try
        {
            // Allow all authenticated users to view course instructor details
            var instructor = await _courseInstructorService.GetCourseInstructorByIdAsync(id);
            if (instructor == null)
                return NotFound();

            return Ok(instructor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course instructor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<IEnumerable<CourseInstructorDto>>> GetCourseInstructorsByCourse(Guid courseId)
    {
        try
        {
            // Allow all authenticated users to view course instructors by course
            var instructors = await _courseInstructorService.GetCourseInstructorsByCourseIdAsync(courseId);
            return Ok(instructors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course instructors for course {CourseId}", courseId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CourseInstructorDto>> CreateCourseInstructor(CreateCourseInstructorDto createDto)
    {
        try
        {
            // Validate user has admin role
            var userRole = GetUserRole();
            if (userRole != "Administrator")
            {
                return Forbid("Only administrators can create course instructors");
            }

            var instructor = await _courseInstructorService.CreateCourseInstructorAsync(createDto);
            return CreatedAtAction(nameof(GetCourseInstructor), new { id = instructor.Id }, instructor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course instructor");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CourseInstructorDto>> UpdateCourseInstructor(Guid id, UpdateCourseInstructorDto updateDto)
    {
        try
        {
            // Validate user has admin role
            var userRole = GetUserRole();
            if (userRole != "Administrator")
            {
                return Forbid("Only administrators can update course instructors");
            }

            var instructor = await _courseInstructorService.UpdateCourseInstructorAsync(id, updateDto);
            if (instructor == null)
                return NotFound();

            return Ok(instructor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course instructor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCourseInstructor(Guid id)
    {
        try
        {
            // Validate user has admin role
            var userRole = GetUserRole();
            if (userRole != "Administrator")
            {
                return Forbid("Only administrators can delete course instructors");
            }

            var deleted = await _courseInstructorService.DeleteCourseInstructorAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course instructor {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private string GetUserRole()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return "";

        var token = authHeader.Substring("Bearer ".Length);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        // Check for role claims (Keycloak uses different claim names)
        var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "realm_access");

        if (roleClaim != null)
        {
            try
            {
                // Parse the JSON to extract roles
                var realmAccess = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(roleClaim.Value);
                if (realmAccess != null && realmAccess.ContainsKey("roles"))
                {
                    var rolesObject = realmAccess["roles"];
                    if (rolesObject != null)
                    {
                        var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(rolesObject.ToString()!);
                        return roles?.FirstOrDefault(r => r == "Administrator" || r == "Member" || r == "Instructor") ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JWT token roles");
            }
        }

        return "";
    }
}
