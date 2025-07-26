using StudentRegistrar.Models;
using StudentRegistrar.Api.DTOs;

namespace StudentRegistrar.Api.Services;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
    Task<StudentDto?> GetStudentByIdAsync(int id);
    Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
    Task<StudentDto?> UpdateStudentAsync(int id, UpdateStudentDto updateStudentDto);
    Task<bool> DeleteStudentAsync(int id);
    Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId);
    Task<IEnumerable<GradeRecordDto>> GetStudentGradesAsync(int studentId);
}

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto?> GetCourseByIdAsync(int id);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto);
    Task<bool> DeleteCourseAsync(int id);
    Task<IEnumerable<EnrollmentDto>> GetCourseEnrollmentsAsync(int courseId);
    Task<IEnumerable<GradeRecordDto>> GetCourseGradesAsync(int courseId);
}

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync();
    Task<EnrollmentDto?> GetEnrollmentByIdAsync(int id);
    Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<bool> DeleteEnrollmentAsync(int id);
    Task<EnrollmentDto?> UpdateEnrollmentStatusAsync(int id, string status);
}

public interface IGradeService
{
    Task<IEnumerable<GradeRecordDto>> GetAllGradesAsync();
    Task<GradeRecordDto?> GetGradeByIdAsync(int id);
    Task<GradeRecordDto> CreateGradeAsync(CreateGradeRecordDto createGradeDto);
    Task<GradeRecordDto?> UpdateGradeAsync(int id, CreateGradeRecordDto updateGradeDto);
    Task<bool> DeleteGradeAsync(int id);
}

public interface IKeycloakService
{
    Task<string> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserRoleAsync(string keycloakId, UserRole role);
    Task DeactivateUserAsync(string keycloakId);
    Task<bool> UserExistsAsync(string email);
}

public interface ICourseInstructorService
{
    Task<IEnumerable<CourseInstructorDto>> GetAllCourseInstructorsAsync();
    Task<CourseInstructorDto?> GetCourseInstructorByIdAsync(Guid id);
    Task<IEnumerable<CourseInstructorDto>> GetCourseInstructorsByCourseIdAsync(Guid courseId);
    Task<CourseInstructorDto> CreateCourseInstructorAsync(CreateCourseInstructorDto createDto);
    Task<CourseInstructorDto?> UpdateCourseInstructorAsync(Guid id, UpdateCourseInstructorDto updateDto);
    Task<bool> DeleteCourseInstructorAsync(Guid id);
}

// Independent Educator Service (replaces CourseInstructor system)
public interface IEducatorService
{
    Task<IEnumerable<EducatorDto>> GetAllEducatorsAsync();
    Task<EducatorDto?> GetEducatorByIdAsync(Guid id);
    Task<IEnumerable<EducatorDto>> GetEducatorsByCourseIdAsync(Guid courseId);
    Task<IEnumerable<EducatorDto>> GetUnassignedEducatorsAsync();
    Task<EducatorDto> CreateEducatorAsync(CreateEducatorDto createDto);
    Task<EducatorDto?> UpdateEducatorAsync(Guid id, UpdateEducatorDto updateDto);
    Task<bool> DeleteEducatorAsync(Guid id);
    Task<bool> DeactivateEducatorAsync(Guid id);
    Task<bool> ActivateEducatorAsync(Guid id);
}

// New Course System Services
public interface ISemesterService
{
    Task<IEnumerable<SemesterDto>> GetAllSemestersAsync();
    Task<SemesterDto?> GetSemesterByIdAsync(Guid id);
    Task<SemesterDto?> GetActiveSemesterAsync();
    Task<SemesterDto> CreateSemesterAsync(CreateSemesterDto createDto);
    Task<SemesterDto?> UpdateSemesterAsync(Guid id, UpdateSemesterDto updateDto);
    Task<bool> DeleteSemesterAsync(Guid id);
}

public interface INewCourseService
{
    Task<IEnumerable<NewCourseDto>> GetAllCoursesAsync();
    Task<IEnumerable<NewCourseDto>> GetCoursesBySemesterAsync(Guid semesterId);
    Task<NewCourseDto?> GetCourseByIdAsync(Guid id);
    Task<NewCourseDto> CreateCourseAsync(CreateNewCourseDto createDto);
    Task<NewCourseDto?> UpdateCourseAsync(Guid id, UpdateNewCourseDto updateDto);
    Task<bool> DeleteCourseAsync(Guid id);
}

public interface IAccountHolderService
{
    Task<AccountHolderDto?> GetAccountHolderByUserIdAsync(string userId);
    Task<AccountHolderDto?> GetAccountHolderByIdAsync(Guid id);
    Task<AccountHolderDto> CreateAccountHolderAsync(CreateAccountHolderDto createDto);
    Task<AccountHolderDto?> UpdateAccountHolderAsync(Guid id, UpdateAccountHolderDto updateDto);
    Task<StudentDto> AddStudentToAccountAsync(Guid accountHolderId, CreateStudentForAccountDto createDto);
    Task<bool> RemoveStudentFromAccountAsync(Guid accountHolderId, Guid studentId);
}
