using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public class CourseInstructorRepository : ICourseInstructorRepository
{
    private readonly StudentRegistrarDbContext _context;

    public CourseInstructorRepository(StudentRegistrarDbContext context)
    {
        _context = context;
    }

    public async Task<CourseInstructor?> GetByIdAsync(Guid id)
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Course)
                .ThenInclude(c => c.Semester)
            .Include(ci => ci.Educator)
            .FirstOrDefaultAsync(ci => ci.Id == id);
    }

    public async Task<CourseInstructor?> GetByCourseAndEducatorAsync(Guid courseId, Guid educatorId)
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Course)
            .Include(ci => ci.Educator)
            .FirstOrDefaultAsync(ci => ci.CourseId == courseId && ci.EducatorId == educatorId);
    }

    public async Task<IEnumerable<CourseInstructor>> GetAllAsync()
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Course)
                .ThenInclude(c => c.Semester)
            .Include(ci => ci.Educator)
            .OrderBy(ci => ci.Course.Semester.StartDate)
            .ThenBy(ci => ci.Course.Code)
            .ThenBy(ci => ci.Educator.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseInstructor>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Educator)
            .Where(ci => ci.CourseId == courseId)
            .OrderBy(ci => ci.IsPrimary ? 0 : 1)
            .ThenBy(ci => ci.Educator.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseInstructor>> GetByInstructorEmailAsync(string email)
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Course)
                .ThenInclude(c => c.Semester)
            .Include(ci => ci.Educator)
            .Where(ci => ci.Educator.Email == email)
            .OrderByDescending(ci => ci.Course.Semester.StartDate)
            .ThenBy(ci => ci.Course.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseInstructor>> GetInstructorsForSemesterAsync(Guid semesterId)
    {
        return await _context.CourseInstructors
            .Include(ci => ci.Course)
            .Include(ci => ci.Educator)
            .Where(ci => ci.Course.SemesterId == semesterId)
            .OrderBy(ci => ci.Course.Code)
            .ThenBy(ci => ci.IsPrimary ? 0 : 1)
            .ThenBy(ci => ci.Educator.LastName)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.CourseInstructors
            .AnyAsync(ci => ci.Educator.Email == email);
    }

    public async Task<CourseInstructor> CreateAsync(CourseInstructor courseInstructor)
    {
        courseInstructor.CreatedAt = DateTime.UtcNow;
        courseInstructor.UpdatedAt = DateTime.UtcNow;
        
        _context.CourseInstructors.Add(courseInstructor);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(courseInstructor.Id) ?? courseInstructor;
    }

    public async Task<CourseInstructor> UpdateAsync(CourseInstructor courseInstructor)
    {
        courseInstructor.UpdatedAt = DateTime.UtcNow;
        
        _context.CourseInstructors.Update(courseInstructor);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(courseInstructor.Id) ?? courseInstructor;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var courseInstructor = await _context.CourseInstructors.FindAsync(id);
        if (courseInstructor == null)
            return false;

        _context.CourseInstructors.Remove(courseInstructor);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.CourseInstructors.AnyAsync(ci => ci.Id == id);
    }
}
