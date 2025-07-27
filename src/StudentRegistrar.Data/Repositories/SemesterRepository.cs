using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly StudentRegistrarDbContext _context;

    public SemesterRepository(StudentRegistrarDbContext context)
    {
        _context = context;
    }

    public async Task<Semester?> GetByIdAsync(Guid id)
    {
        return await _context.Semesters
            .Include(s => s.Courses)
                .ThenInclude(c => c.CourseInstructors)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Semester?> GetByCodeAsync(string code)
    {
        return await _context.Semesters
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<Semester?> GetActiveAsync()
    {
        return await _context.Semesters
            .Include(s => s.Courses)
                .ThenInclude(c => c.CourseInstructors)
            .FirstOrDefaultAsync(s => s.IsActive);
    }

    public async Task<IEnumerable<Semester>> GetAllAsync()
    {
        return await _context.Semesters
            .Include(s => s.Courses)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<Semester> CreateAsync(Semester semester)
    {
        semester.CreatedAt = DateTime.UtcNow;
        semester.UpdatedAt = DateTime.UtcNow;
        
        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(semester.Id) ?? semester;
    }

    public async Task<Semester> UpdateAsync(Semester semester)
    {
        semester.UpdatedAt = DateTime.UtcNow;
        
        _context.Semesters.Update(semester);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(semester.Id) ?? semester;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var semester = await _context.Semesters.FindAsync(id);
        if (semester == null)
            return false;

        _context.Semesters.Remove(semester);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<Semester>> GetSemestersWithCoursesAsync()
    {
        return await _context.Semesters
            .Include(s => s.Courses)
                .ThenInclude(c => c.CourseInstructors)
            .Include(s => s.Courses)
                .ThenInclude(c => c.Enrollments)
            .Where(s => s.Courses.Any())
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Semesters.AnyAsync(s => s.Id == id);
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _context.Semesters.AnyAsync(s => s.Code == code);
    }

    public async Task<Semester> SetActiveAsync(Guid id)
    {
        // First, deactivate all semesters
        await _context.Semesters
            .Where(s => s.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

        // Then activate the specified semester
        var semester = await _context.Semesters.FindAsync(id);
        if (semester != null)
        {
            semester.IsActive = true;
            semester.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Semester not found");
    }
}
