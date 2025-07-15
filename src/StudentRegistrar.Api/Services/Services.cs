using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudentRegistrar.Api.Services;

public class StudentService : IStudentService
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;

    public StudentService(StudentRegistrarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var students = await _context.Students
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }

    public async Task<StudentDto?> GetStudentByIdAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        return student != null ? _mapper.Map<StudentDto>(student) : null;
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        var student = _mapper.Map<Student>(createStudentDto);
        
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<StudentDto>(student);
    }

    public async Task<StudentDto?> UpdateStudentAsync(int id, UpdateStudentDto updateStudentDto)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return null;

        _mapper.Map(updateStudentDto, student);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<StudentDto>(student);
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId)
    {
        var enrollments = await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course)
            .Include(e => e.Student)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<IEnumerable<GradeRecordDto>> GetStudentGradesAsync(int studentId)
    {
        var grades = await _context.GradeRecords
            .Where(g => g.StudentId == studentId)
            .Include(g => g.Course)
            .Include(g => g.Student)
            .OrderByDescending(g => g.GradeDate)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<GradeRecordDto>>(grades);
    }
}

public class CourseService : ICourseService
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;

    public CourseService(StudentRegistrarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _context.Courses
            .OrderBy(c => c.AcademicYear)
            .ThenBy(c => c.Semester)
            .ThenBy(c => c.Code)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        return course != null ? _mapper.Map<CourseDto>(course) : null;
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var course = _mapper.Map<Course>(createCourseDto);
        
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return null;

        _mapper.Map(updateCourseDto, course);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetCourseEnrollmentsAsync(int courseId)
    {
        var enrollments = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<IEnumerable<GradeRecordDto>> GetCourseGradesAsync(int courseId)
    {
        var grades = await _context.GradeRecords
            .Where(g => g.CourseId == courseId)
            .Include(g => g.Student)
            .Include(g => g.Course)
            .OrderBy(g => g.Student.LastName)
            .ThenBy(g => g.Student.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<GradeRecordDto>>(grades);
    }
}

public class EnrollmentService : IEnrollmentService
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;

    public EnrollmentService(StudentRegistrarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync()
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        return enrollment != null ? _mapper.Map<EnrollmentDto>(enrollment) : null;
    }

    public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
        
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        
        // Load the related entities
        await _context.Entry(enrollment)
            .Reference(e => e.Student)
            .LoadAsync();
        await _context.Entry(enrollment)
            .Reference(e => e.Course)
            .LoadAsync();
        
        return _mapper.Map<EnrollmentDto>(enrollment);
    }

    public async Task<bool> DeleteEnrollmentAsync(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
            return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EnrollmentDto?> UpdateEnrollmentStatusAsync(int id, string status)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (enrollment == null)
            return null;

        enrollment.Status = status;
        if (status == "Completed")
            enrollment.CompletionDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return _mapper.Map<EnrollmentDto>(enrollment);
    }
}

public class GradeService : IGradeService
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;

    public GradeService(StudentRegistrarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GradeRecordDto>> GetAllGradesAsync()
    {
        var grades = await _context.GradeRecords
            .Include(g => g.Student)
            .Include(g => g.Course)
            .OrderByDescending(g => g.GradeDate)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<GradeRecordDto>>(grades);
    }

    public async Task<GradeRecordDto?> GetGradeByIdAsync(int id)
    {
        var grade = await _context.GradeRecords
            .Include(g => g.Student)
            .Include(g => g.Course)
            .FirstOrDefaultAsync(g => g.Id == id);
        
        return grade != null ? _mapper.Map<GradeRecordDto>(grade) : null;
    }

    public async Task<GradeRecordDto> CreateGradeAsync(CreateGradeRecordDto createGradeDto)
    {
        var grade = _mapper.Map<GradeRecord>(createGradeDto);
        
        _context.GradeRecords.Add(grade);
        await _context.SaveChangesAsync();
        
        // Load the related entities
        await _context.Entry(grade)
            .Reference(g => g.Student)
            .LoadAsync();
        await _context.Entry(grade)
            .Reference(g => g.Course)
            .LoadAsync();
        
        return _mapper.Map<GradeRecordDto>(grade);
    }

    public async Task<GradeRecordDto?> UpdateGradeAsync(int id, CreateGradeRecordDto updateGradeDto)
    {
        var grade = await _context.GradeRecords
            .Include(g => g.Student)
            .Include(g => g.Course)
            .FirstOrDefaultAsync(g => g.Id == id);
        
        if (grade == null)
            return null;

        _mapper.Map(updateGradeDto, grade);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<GradeRecordDto>(grade);
    }

    public async Task<bool> DeleteGradeAsync(int id)
    {
        var grade = await _context.GradeRecords.FindAsync(id);
        if (grade == null)
            return false;

        _context.GradeRecords.Remove(grade);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;
    private readonly string _keycloakUrl;
    private readonly string _realm;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public KeycloakService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _keycloakUrl = configuration.GetConnectionString("keycloak") ?? "http://localhost:8080";
        _realm = configuration["Keycloak:Realm"] ?? "student-registrar";
        _clientId = configuration["Keycloak:ClientId"] ?? "student-registrar";
        _clientSecret = configuration["Keycloak:ClientSecret"] ?? "";
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var accessToken = await GetAdminAccessTokenAsync();
            
            var userPayload = new
            {
                username = request.Email,
                email = request.Email,
                firstName = request.FirstName,
                lastName = request.LastName,
                enabled = true,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = request.Password,
                        temporary = false
                    }
                }
            };

            var json = JsonSerializer.Serialize(userPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.PostAsync($"{_keycloakUrl}/admin/realms/{_realm}/users", content);
            
            if (response.IsSuccessStatusCode)
            {
                var locationHeader = response.Headers.Location?.ToString();
                if (locationHeader != null)
                {
                    var userId = locationHeader.Split('/').Last();
                    
                    // Assign role
                    await AssignUserRoleAsync(userId, request.Role, accessToken);
                    
                    return userId;
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create user in Keycloak: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user in Keycloak");
            throw;
        }
    }

    public async Task UpdateUserRoleAsync(string keycloakId, UserRole role)
    {
        try
        {
            var accessToken = await GetAdminAccessTokenAsync();
            await AssignUserRoleAsync(keycloakId, role, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role in Keycloak");
            throw;
        }
    }

    public async Task DeactivateUserAsync(string keycloakId)
    {
        try
        {
            var accessToken = await GetAdminAccessTokenAsync();
            
            var userPayload = new { enabled = false };
            var json = JsonSerializer.Serialize(userPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.PutAsync($"{_keycloakUrl}/admin/realms/{_realm}/users/{keycloakId}", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to deactivate user in Keycloak: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user in Keycloak");
            throw;
        }
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            var accessToken = await GetAdminAccessTokenAsync();
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync($"{_keycloakUrl}/admin/realms/{_realm}/users?email={Uri.EscapeDataString(email)}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<JsonElement[]>(content);
                return users.Length > 0;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists in Keycloak");
            return false;
        }
    }

    private async Task<string> GetAdminAccessTokenAsync()
    {
        var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";
        
        var parameters = new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret)
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(tokenEndpoint, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get admin access token: {response.StatusCode} - {errorContent}");
        }
        
        var tokenResponse = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenResponse);
        
        return tokenData.GetProperty("access_token").GetString() ?? throw new Exception("No access token received");
    }

    private async Task AssignUserRoleAsync(string userId, UserRole role, string accessToken)
    {
        var roleName = role.ToString();
        
        // Get role representation
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var roleResponse = await _httpClient.GetAsync($"{_keycloakUrl}/admin/realms/{_realm}/roles/{roleName}");
        
        if (!roleResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Role {RoleName} not found in Keycloak, skipping role assignment", roleName);
            return;
        }
        
        var roleContent = await roleResponse.Content.ReadAsStringAsync();
        var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);
        
        var roleAssignment = new[]
        {
            new
            {
                id = roleData.GetProperty("id").GetString(),
                name = roleData.GetProperty("name").GetString()
            }
        };
        
        var json = JsonSerializer.Serialize(roleAssignment);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        await _httpClient.PostAsync($"{_keycloakUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm", content);
    }
}
