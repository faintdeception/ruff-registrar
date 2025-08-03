using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Data;
using StudentRegistrar.Data.Repositories;
using StudentRegistrar.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudentRegistrar.Api.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAccountHolderRepository _accountHolderRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;

    public StudentService(
        IStudentRepository studentRepository,
        IAccountHolderRepository accountHolderRepository,
        IEnrollmentRepository enrollmentRepository,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _accountHolderRepository = accountHolderRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var students = await _studentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }

    public async Task<StudentDto?> GetStudentByIdAsync(Guid id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        return student != null ? _mapper.Map<StudentDto>(student) : null;
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        var student = _mapper.Map<Student>(createStudentDto);
        var createdStudent = await _studentRepository.CreateAsync(student);
        return _mapper.Map<StudentDto>(createdStudent);
    }

    public async Task<StudentDto?> UpdateStudentAsync(Guid id, UpdateStudentDto updateStudentDto)
    {
        var existingStudent = await _studentRepository.GetByIdAsync(id);
        if (existingStudent == null)
            return null;

        _mapper.Map(updateStudentDto, existingStudent);
        var updatedStudent = await _studentRepository.UpdateAsync(existingStudent);
        return _mapper.Map<StudentDto>(updatedStudent);
    }

    public async Task<bool> DeleteStudentAsync(Guid id)
    {
        return await _studentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepository.GetByStudentAsync(studentId);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<IEnumerable<StudentDto>> GetStudentsByAccountHolderAsync(Guid accountHolderId)
    {
        var students = await _studentRepository.GetByAccountHolderAsync(accountHolderId);
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }
}

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IMapper mapper)
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetAllEnrollmentsAsync()
    {
        var enrollments = await _enrollmentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(Guid id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        return enrollment != null ? _mapper.Map<EnrollmentDto>(enrollment) : null;
    }

    public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
        var createdEnrollment = await _enrollmentRepository.CreateAsync(enrollment);
        return _mapper.Map<EnrollmentDto>(createdEnrollment);
    }

    public async Task<bool> DeleteEnrollmentAsync(Guid id)
    {
        return await _enrollmentRepository.DeleteAsync(id);
    }

    public async Task<EnrollmentDto?> UpdateEnrollmentStatusAsync(Guid id, string status)
    {
        var existingEnrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (existingEnrollment == null)
            return null;

        // Update the enrollment type based on status
        if (Enum.TryParse<EnrollmentType>(status, out var enrollmentType))
        {
            existingEnrollment.EnrollmentType = enrollmentType;
            var updatedEnrollment = await _enrollmentRepository.UpdateAsync(existingEnrollment);
            return _mapper.Map<EnrollmentDto>(updatedEnrollment);
        }

        return null;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepository.GetByStudentAsync(studentId);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseAsync(Guid courseId)
    {
        var enrollments = await _enrollmentRepository.GetByCourseAsync(courseId);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsBySemesterAsync(Guid semesterId)
    {
        var enrollments = await _enrollmentRepository.GetBySemesterAsync(semesterId);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }
}

public class CourseServiceV2 : ICourseServiceV2
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICourseInstructorRepository _courseInstructorRepository;
    private readonly IAccountHolderRepository _accountHolderRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;

    public CourseServiceV2(
        ICourseRepository courseRepository, 
        ICourseInstructorRepository courseInstructorRepository,
        IAccountHolderRepository accountHolderRepository,
        IRoomRepository roomRepository,
        IMapper mapper)
    {
        _courseRepository = courseRepository;
        _courseInstructorRepository = courseInstructorRepository;
        _accountHolderRepository = accountHolderRepository;
        _roomRepository = roomRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesBySemesterAsync(Guid semesterId)
    {
        var courses = await _courseRepository.GetBySemesterAsync(semesterId);
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        return course != null ? _mapper.Map<CourseDto>(course) : null;
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createDto)
    {
        var course = _mapper.Map<Course>(createDto);
        var createdCourse = await _courseRepository.CreateAsync(course);
        return _mapper.Map<CourseDto>(createdCourse);
    }

    public async Task<CourseDto?> UpdateCourseAsync(Guid id, UpdateCourseDto updateDto)
    {
        var existingCourse = await _courseRepository.GetByIdAsync(id);
        if (existingCourse == null)
            return null;

        _mapper.Map(updateDto, existingCourse);
        var updatedCourse = await _courseRepository.UpdateAsync(existingCourse);
        return _mapper.Map<CourseDto>(updatedCourse);
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        return await _courseRepository.DeleteAsync(id);
    }

    // Instructor management methods
    public async Task<IEnumerable<CourseInstructorDto>> GetCourseInstructorsAsync(Guid courseId)
    {
        var instructors = await _courseInstructorRepository.GetByCourseIdAsync(courseId);
        return _mapper.Map<IEnumerable<CourseInstructorDto>>(instructors);
    }

    public async Task<CourseInstructorDto> AddInstructorAsync(CreateCourseInstructorDto createDto)
    {
        var instructor = _mapper.Map<CourseInstructor>(createDto);
        
        // If AccountHolderId is provided, populate name and email from the account holder
        if (createDto.AccountHolderId.HasValue)
        {
            var accountHolder = await _accountHolderRepository.GetByIdAsync(createDto.AccountHolderId.Value);
            if (accountHolder != null)
            {
                instructor.FirstName = accountHolder.FirstName;
                instructor.LastName = accountHolder.LastName;
                instructor.Email = accountHolder.EmailAddress;
            }
        }

        var createdInstructor = await _courseInstructorRepository.CreateAsync(instructor);
        return _mapper.Map<CourseInstructorDto>(createdInstructor);
    }

    public async Task<CourseInstructorDto?> UpdateInstructorAsync(Guid instructorId, UpdateCourseInstructorDto updateDto)
    {
        var existingInstructor = await _courseInstructorRepository.GetByIdAsync(instructorId);
        if (existingInstructor == null)
            return null;

        _mapper.Map(updateDto, existingInstructor);
        var updatedInstructor = await _courseInstructorRepository.UpdateAsync(existingInstructor);
        return _mapper.Map<CourseInstructorDto>(updatedInstructor);
    }

    public async Task<bool> RemoveInstructorAsync(Guid instructorId)
    {
        return await _courseInstructorRepository.DeleteAsync(instructorId);
    }

    public async Task<IEnumerable<AccountHolderDto>> GetAvailableMembersAsync()
    {
        var accountHolders = await _accountHolderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AccountHolderDto>>(accountHolders);
    }

}

public class AccountHolderService : IAccountHolderService
{
    private readonly IAccountHolderRepository _accountHolderRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;

    public AccountHolderService(
        IAccountHolderRepository accountHolderRepository,
        IStudentRepository studentRepository,
        IMapper mapper)
    {
        _accountHolderRepository = accountHolderRepository;
        _studentRepository = studentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AccountHolderDto>> GetAllAccountHoldersAsync()
    {
        var accountHolders = await _accountHolderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AccountHolderDto>>(accountHolders);
    }

    public async Task<AccountHolderDto?> GetAccountHolderByUserIdAsync(string userId)
    {
        var accountHolder = await _accountHolderRepository.GetByKeycloakUserIdAsync(userId);
        return accountHolder != null ? _mapper.Map<AccountHolderDto>(accountHolder) : null;
    }

    public async Task<AccountHolderDto?> GetAccountHolderByIdAsync(Guid id)
    {
        var accountHolder = await _accountHolderRepository.GetByIdAsync(id);
        return accountHolder != null ? _mapper.Map<AccountHolderDto>(accountHolder) : null;
    }

    public async Task<AccountHolderDto> CreateAccountHolderAsync(CreateAccountHolderDto createDto)
    {
        var accountHolder = _mapper.Map<AccountHolder>(createDto);
        var createdAccountHolder = await _accountHolderRepository.CreateAsync(accountHolder);
        return _mapper.Map<AccountHolderDto>(createdAccountHolder);
    }

    public async Task<AccountHolderDto> CreateAccountHolderAsync(CreateAccountHolderDto createDto, string? keycloakUserId)
    {
        var accountHolder = _mapper.Map<AccountHolder>(createDto);
        if (!string.IsNullOrEmpty(keycloakUserId))
        {
            accountHolder.KeycloakUserId = keycloakUserId;
        }
        var createdAccountHolder = await _accountHolderRepository.CreateAsync(accountHolder);
        return _mapper.Map<AccountHolderDto>(createdAccountHolder);
    }

    public async Task<AccountHolderDto?> UpdateAccountHolderAsync(Guid id, UpdateAccountHolderDto updateDto)
    {
        var existingAccountHolder = await _accountHolderRepository.GetByIdAsync(id);
        if (existingAccountHolder == null)
            return null;

        _mapper.Map(updateDto, existingAccountHolder);
        var updatedAccountHolder = await _accountHolderRepository.UpdateAsync(existingAccountHolder);
        return _mapper.Map<AccountHolderDto>(updatedAccountHolder);
    }

    public async Task<StudentDto> AddStudentToAccountAsync(Guid accountHolderId, CreateStudentForAccountDto createDto)
    {
        var student = _mapper.Map<Student>(createDto);
        student.AccountHolderId = accountHolderId;
        
        var createdStudent = await _studentRepository.CreateAsync(student);
        return _mapper.Map<StudentDto>(createdStudent);
    }

    public async Task<bool> RemoveStudentFromAccountAsync(Guid accountHolderId, Guid studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null || student.AccountHolderId != accountHolderId)
            return false;

        return await _studentRepository.DeleteAsync(studentId);
    }
}

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;
    private readonly IMapper _mapper;

    public SemesterService(ISemesterRepository semesterRepository, IMapper mapper)
    {
        _semesterRepository = semesterRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SemesterDto>> GetAllSemestersAsync()
    {
        var semesters = await _semesterRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SemesterDto>>(semesters);
    }

    public async Task<SemesterDto?> GetSemesterByIdAsync(Guid id)
    {
        var semester = await _semesterRepository.GetByIdAsync(id);
        return semester != null ? _mapper.Map<SemesterDto>(semester) : null;
    }

    public async Task<SemesterDto?> GetActiveSemesterAsync()
    {
        var semester = await _semesterRepository.GetActiveAsync();
        return semester != null ? _mapper.Map<SemesterDto>(semester) : null;
    }

    public async Task<SemesterDto> CreateSemesterAsync(CreateSemesterDto createDto)
    {
        var semester = _mapper.Map<Semester>(createDto);
        var createdSemester = await _semesterRepository.CreateAsync(semester);
        return _mapper.Map<SemesterDto>(createdSemester);
    }

    public async Task<SemesterDto?> UpdateSemesterAsync(Guid id, UpdateSemesterDto updateDto)
    {
        var existingSemester = await _semesterRepository.GetByIdAsync(id);
        if (existingSemester == null)
            return null;

        _mapper.Map(updateDto, existingSemester);
        var updatedSemester = await _semesterRepository.UpdateAsync(existingSemester);
        return _mapper.Map<SemesterDto>(updatedSemester);
    }

    public async Task<bool> DeleteSemesterAsync(Guid id)
    {
        return await _semesterRepository.DeleteAsync(id);
    }
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public PaymentService(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _paymentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByAccountHolderAsync(Guid accountHolderId)
    {
        var payments = await _paymentRepository.GetByAccountHolderIdAsync(accountHolderId);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByEnrollmentAsync(Guid enrollmentId)
    {
        var payments = await _paymentRepository.GetByEnrollmentIdAsync(enrollmentId);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByTypeAsync(PaymentType paymentType)
    {
        var payments = await _paymentRepository.GetByTypeAsync(paymentType);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createDto)
    {
        var payment = _mapper.Map<Payment>(createDto);
        var createdPayment = await _paymentRepository.CreateAsync(payment);
        return _mapper.Map<PaymentDto>(createdPayment);
    }

    public async Task<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentDto updateDto)
    {
        var existingPayment = await _paymentRepository.GetByIdAsync(id);
        if (existingPayment == null)
            return null;

        _mapper.Map(updateDto, existingPayment);
        var updatedPayment = await _paymentRepository.UpdateAsync(existingPayment);
        return _mapper.Map<PaymentDto>(updatedPayment);
    }

    public async Task<bool> DeletePaymentAsync(Guid id)
    {
        return await _paymentRepository.DeleteAsync(id);
    }

    public async Task<decimal> GetTotalPaidByAccountHolderAsync(Guid accountHolderId, PaymentType? type = null)
    {
        return await _paymentRepository.GetTotalPaidByAccountHolderAsync(accountHolderId, type);
    }

    public async Task<decimal> GetTotalPaidByEnrollmentAsync(Guid enrollmentId)
    {
        return await _paymentRepository.GetTotalPaidByEnrollmentAsync(enrollmentId);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(Guid accountHolderId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var payments = await _paymentRepository.GetPaymentHistoryAsync(accountHolderId, fromDate, toDate);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }
}

public class CourseInstructorService : ICourseInstructorService
{
    private readonly ICourseInstructorRepository _courseInstructorRepository;
    private readonly IMapper _mapper;

    public CourseInstructorService(ICourseInstructorRepository courseInstructorRepository, IMapper mapper)
    {
        _courseInstructorRepository = courseInstructorRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseInstructorDto>> GetAllCourseInstructorsAsync()
    {
        var courseInstructors = await _courseInstructorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourseInstructorDto>>(courseInstructors);
    }

    public async Task<CourseInstructorDto?> GetCourseInstructorByIdAsync(Guid id)
    {
        var courseInstructor = await _courseInstructorRepository.GetByIdAsync(id);
        return courseInstructor != null ? _mapper.Map<CourseInstructorDto>(courseInstructor) : null;
    }

    public async Task<IEnumerable<CourseInstructorDto>> GetCourseInstructorsByCourseIdAsync(Guid courseId)
    {
        var courseInstructors = await _courseInstructorRepository.GetByCourseIdAsync(courseId);
        return _mapper.Map<IEnumerable<CourseInstructorDto>>(courseInstructors);
    }

    public async Task<CourseInstructorDto> CreateCourseInstructorAsync(CreateCourseInstructorDto createDto)
    {
        var courseInstructor = _mapper.Map<CourseInstructor>(createDto);
        var createdCourseInstructor = await _courseInstructorRepository.CreateAsync(courseInstructor);
        return _mapper.Map<CourseInstructorDto>(createdCourseInstructor);
    }

    public async Task<CourseInstructorDto?> UpdateCourseInstructorAsync(Guid id, UpdateCourseInstructorDto updateDto)
    {
        var existingCourseInstructor = await _courseInstructorRepository.GetByIdAsync(id);
        if (existingCourseInstructor == null)
            return null;

        _mapper.Map(updateDto, existingCourseInstructor);
        var updatedCourseInstructor = await _courseInstructorRepository.UpdateAsync(existingCourseInstructor);
        return _mapper.Map<CourseInstructorDto>(updatedCourseInstructor);
    }

    public async Task<bool> DeleteCourseInstructorAsync(Guid id)
    {
        return await _courseInstructorRepository.DeleteAsync(id);
    }
}

public class GradeService : IGradeService
{
    private readonly StudentRegistrar.Data.Repositories.IGradeRepository _gradeRepository;
    private readonly IMapper _mapper;

    public GradeService(
        StudentRegistrar.Data.Repositories.IGradeRepository gradeRepository,
        IMapper mapper)
    {
        _gradeRepository = gradeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GradeRecordDto>> GetAllGradesAsync()
    {
        var grades = await _gradeRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<GradeRecordDto>>(grades);
    }

    public async Task<GradeRecordDto?> GetGradeByIdAsync(int id)
    {
        var grade = await _gradeRepository.GetByIdAsync(id);
        return grade == null ? null : _mapper.Map<GradeRecordDto>(grade);
    }

    public async Task<IEnumerable<GradeRecordDto>> GetGradesByStudentAsync(Guid studentId)
    {
        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);
        return _mapper.Map<IEnumerable<GradeRecordDto>>(grades);
    }

    public async Task<IEnumerable<GradeRecordDto>> GetGradesByCourseAsync(Guid courseId)
    {
        var grades = await _gradeRepository.GetByCourseIdAsync(courseId);
        return _mapper.Map<IEnumerable<GradeRecordDto>>(grades);
    }

    public async Task<GradeRecordDto> CreateGradeAsync(CreateGradeRecordDto createGradeDto)
    {
        var grade = _mapper.Map<GradeRecord>(createGradeDto);
        var createdGrade = await _gradeRepository.CreateAsync(grade);
        return _mapper.Map<GradeRecordDto>(createdGrade);
    }

    public async Task<GradeRecordDto?> UpdateGradeAsync(int id, CreateGradeRecordDto updateGradeDto)
    {
        var existingGrade = await _gradeRepository.GetByIdAsync(id);
        if (existingGrade == null)
            return null;

        _mapper.Map(updateGradeDto, existingGrade);
        var updatedGrade = await _gradeRepository.UpdateAsync(existingGrade);
        return _mapper.Map<GradeRecordDto>(updatedGrade);
    }

    public async Task<bool> DeleteGradeAsync(int id)
    {
        return await _gradeRepository.DeleteAsync(id);
    }
}

public class EducatorService : IEducatorService
{
    private readonly StudentRegistrar.Data.Repositories.IEducatorRepository _educatorRepository;
    private readonly IMapper _mapper;

    public EducatorService(
        StudentRegistrar.Data.Repositories.IEducatorRepository educatorRepository,
        IMapper mapper)
    {
        _educatorRepository = educatorRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EducatorDto>> GetAllEducatorsAsync()
    {
        var educators = await _educatorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<EducatorDto>>(educators);
    }

    public async Task<EducatorDto?> GetEducatorByIdAsync(Guid id)
    {
        var educator = await _educatorRepository.GetByIdAsync(id);
        return educator == null ? null : _mapper.Map<EducatorDto>(educator);
    }

    public async Task<IEnumerable<EducatorDto>> GetEducatorsByCourseIdAsync(Guid courseId)
    {
        var educators = await _educatorRepository.GetByCourseIdAsync(courseId);
        return _mapper.Map<IEnumerable<EducatorDto>>(educators);
    }

    public async Task<IEnumerable<EducatorDto>> GetUnassignedEducatorsAsync()
    {
        var educators = await _educatorRepository.GetUnassignedAsync();
        return _mapper.Map<IEnumerable<EducatorDto>>(educators);
    }

    public async Task<EducatorDto> CreateEducatorAsync(CreateEducatorDto createDto)
    {
        var educator = _mapper.Map<Educator>(createDto);
        educator.IsActive = true; // Set as active by default
        var createdEducator = await _educatorRepository.CreateAsync(educator);
        return _mapper.Map<EducatorDto>(createdEducator);
    }

    public async Task<EducatorDto?> UpdateEducatorAsync(Guid id, UpdateEducatorDto updateDto)
    {
        var existingEducator = await _educatorRepository.GetByIdAsync(id);
        if (existingEducator == null)
            return null;

        _mapper.Map(updateDto, existingEducator);
        var updatedEducator = await _educatorRepository.UpdateAsync(existingEducator);
        return _mapper.Map<EducatorDto>(updatedEducator);
    }

    public async Task<bool> DeleteEducatorAsync(Guid id)
    {
        return await _educatorRepository.DeleteAsync(id);
    }

    public async Task<bool> DeactivateEducatorAsync(Guid id)
    {
        return await _educatorRepository.DeactivateAsync(id);
    }

    public async Task<bool> ActivateEducatorAsync(Guid id)
    {
        return await _educatorRepository.ActivateAsync(id);
    }
}

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;

    public KeycloakService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // TODO: Implement actual Keycloak API integration
            // For now, return a mock Keycloak ID
            _logger.LogInformation("Creating user with email: {Email}", request.Email);
            
            // This would be replaced with actual Keycloak REST API calls
            var mockKeycloakId = Guid.NewGuid().ToString();
            _logger.LogInformation("Created user with Keycloak ID: {KeycloakId}", mockKeycloakId);
            
            return mockKeycloakId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user in Keycloak for email: {Email}", request.Email);
            throw;
        }
    }

    public async Task UpdateUserRoleAsync(string keycloakId, UserRole role)
    {
        try
        {
            // TODO: Implement actual Keycloak API integration
            _logger.LogInformation("Updating user role for Keycloak ID: {KeycloakId} to role: {Role}", keycloakId, role);
            
            // This would be replaced with actual Keycloak REST API calls
            await Task.Delay(100); // Mock delay
            
            _logger.LogInformation("Successfully updated user role for Keycloak ID: {KeycloakId}", keycloakId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user role for Keycloak ID: {KeycloakId}", keycloakId);
            throw;
        }
    }

    public async Task DeactivateUserAsync(string keycloakId)
    {
        try
        {
            // TODO: Implement actual Keycloak API integration
            _logger.LogInformation("Deactivating user for Keycloak ID: {KeycloakId}", keycloakId);
            
            // This would be replaced with actual Keycloak REST API calls
            await Task.Delay(100); // Mock delay
            
            _logger.LogInformation("Successfully deactivated user for Keycloak ID: {KeycloakId}", keycloakId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate user for Keycloak ID: {KeycloakId}", keycloakId);
            throw;
        }
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            // TODO: Implement actual Keycloak API integration
            _logger.LogInformation("Checking if user exists with email: {Email}", email);
            
            // This would be replaced with actual Keycloak REST API calls
            await Task.Delay(50); // Mock delay
            
            // For now, return false (user doesn't exist)
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user exists for email: {Email}", email);
            throw;
        }
    }
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;

    public RoomService(IRoomRepository roomRepository, IMapper mapper)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(Guid id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        return room == null ? null : _mapper.Map<RoomDto>(room);
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsByTypeAsync(RoomType roomType)
    {
        var rooms = await _roomRepository.GetByTypeAsync(roomType);
        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto createDto)
    {
        // Check if room name already exists
        var existingRoom = await _roomRepository.GetByNameAsync(createDto.Name);
        if (existingRoom != null)
        {
            throw new InvalidOperationException($"A room with the name '{createDto.Name}' already exists.");
        }

        var room = _mapper.Map<Room>(createDto);
        var createdRoom = await _roomRepository.CreateAsync(room);
        return _mapper.Map<RoomDto>(createdRoom);
    }

    public async Task<RoomDto?> UpdateRoomAsync(Guid id, UpdateRoomDto updateDto)
    {
        var existingRoom = await _roomRepository.GetByIdAsync(id);
        if (existingRoom == null)
            return null;

        // Check if name is being changed and if it conflicts with another room
        if (existingRoom.Name != updateDto.Name)
        {
            var nameExists = await _roomRepository.NameExistsAsync(updateDto.Name, id);
            if (nameExists)
            {
                throw new InvalidOperationException($"A room with the name '{updateDto.Name}' already exists.");
            }
        }

        _mapper.Map(updateDto, existingRoom);
        var updatedRoom = await _roomRepository.UpdateAsync(existingRoom);
        return _mapper.Map<RoomDto>(updatedRoom);
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        // Check if room is in use by any courses
        var isInUse = await _roomRepository.IsRoomInUseAsync(id);
        if (isInUse)
        {
            throw new InvalidOperationException("Cannot delete room because it is currently assigned to one or more courses.");
        }

        return await _roomRepository.DeleteAsync(id);
    }

    public async Task<bool> IsRoomInUseAsync(Guid roomId)
    {
        return await _roomRepository.IsRoomInUseAsync(roomId);
    }
}

