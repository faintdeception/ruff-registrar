using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly StudentRegistrarDbContext _context;

    public CourseRepository(StudentRegistrarDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _context.Courses
            .Include(c => c.Semester)
            .Include(c => c.CourseInstructors)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Course?> GetByCodeAsync(string code, Guid semesterId)
    {
        return await _context.Courses
            .Include(c => c.Semester)
            .Include(c => c.CourseInstructors)
            .FirstOrDefaultAsync(c => c.Code == code && c.SemesterId == semesterId);
    }

    public async Task<IEnumerable<Course>> GetBySemesterAsync(Guid semesterId)
    {
        return await _context.Courses
            .Include(c => c.CourseInstructors)
            .Include(c => c.Enrollments)
            .Where(c => c.SemesterId == semesterId)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetByPeriodAsync(Guid semesterId, string periodCode)
    {
        return await _context.Courses
            .Include(c => c.CourseInstructors)
            .Where(c => c.SemesterId == semesterId && c.PeriodCode == periodCode)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses
            .Include(c => c.Semester)
            .Include(c => c.CourseInstructors)
            .OrderBy(c => c.Semester.StartDate)
            .ThenBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(course.Id) ?? course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(course.Id) ?? course;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync(Guid? semesterId = null)
    {
        var query = _context.Courses
            .Include(c => c.CourseInstructors)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .Where(c => c.Enrollments.Any());

        if (semesterId.HasValue)
        {
            query = query.Where(c => c.SemesterId == semesterId.Value);
        }

        return await query
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Courses.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid semesterId)
    {
        return await _context.Courses.AnyAsync(c => c.Code == code && c.SemesterId == semesterId);
    }

    public async Task<int> GetEnrollmentCountAsync(Guid courseId)
    {
        return await _context.Enrollments
            .CountAsync(e => e.CourseId == courseId);
    }

    public async Task<int> GetAvailableSpotsAsync(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return 0;

        return course.MaxCapacity - course.Enrollments.Count;
    }
}
