using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Data;
using StudentRegistrar.Models;

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
