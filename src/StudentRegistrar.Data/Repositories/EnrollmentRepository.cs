using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly StudentRegistrarDbContext _context;

    public EnrollmentRepository(StudentRegistrarDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.AccountHolder)
            .Include(e => e.Course)
                .ThenInclude(c => c.CourseInstructors)
            .Include(e => e.Semester)
            .Include(e => e.Payments)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId, Guid? semesterId = null)
    {
        var query = _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Semester)
            .Include(e => e.Student)
            .Where(e => e.StudentId == studentId);

        if (semesterId.HasValue)
        {
            query = query.Where(e => e.Course.SemesterId == semesterId.Value);
        }

        return await query
            .OrderByDescending(e => e.Course.Semester.StartDate)
            .ThenBy(e => e.Course.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(Guid studentId)
    {
        return await GetByStudentIdAsync(studentId);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.EnrollmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseAsync(Guid courseId)
    {
        return await GetByCourseIdAsync(courseId);
    }

    public async Task<IEnumerable<Enrollment>> GetBySemesterAsync(Guid semesterId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.AccountHolder)
            .Include(e => e.Course)
            .Include(e => e.Payments)
            .Where(e => e.SemesterId == semesterId)
            .OrderBy(e => e.Student.LastName)
            .ThenBy(e => e.Student.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _context.Enrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.AccountHolder)
            .Include(e => e.Course)
            .Include(e => e.Semester)
            .OrderBy(e => e.EnrollmentDate)
            .ToListAsync();
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        enrollment.CreatedAt = DateTime.UtcNow;
        enrollment.UpdatedAt = DateTime.UtcNow;
        
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(enrollment.Id) ?? enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        enrollment.UpdatedAt = DateTime.UtcNow;
        
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(enrollment.Id) ?? enrollment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
            return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<Enrollment>> GetByTypeAsync(Guid courseId, EnrollmentType type)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.AccountHolder)
            .Where(e => e.CourseId == courseId && e.EnrollmentType == type)
            .OrderBy(e => e.WaitlistPosition ?? 0)
            .ThenBy(e => e.EnrollmentDate)
            .ToListAsync();
    }

    public async Task<int> GetEnrollmentCountAsync(Guid courseId, EnrollmentType type)
    {
        return await _context.Enrollments
            .CountAsync(e => e.CourseId == courseId && e.EnrollmentType == type);
    }

    public async Task<int> GetNextWaitlistPositionAsync(Guid courseId)
    {
        var maxPosition = await _context.Enrollments
            .Where(e => e.CourseId == courseId && e.EnrollmentType == EnrollmentType.Waitlisted)
            .MaxAsync(e => (int?)e.WaitlistPosition);

        return (maxPosition ?? 0) + 1;
    }

    public async Task<bool> HasEnrollmentAsync(Guid studentId, Guid courseId, EnrollmentType? type = null)
    {
        var query = _context.Enrollments
            .Where(e => e.StudentId == studentId && e.CourseId == courseId);

        if (type.HasValue)
        {
            query = query.Where(e => e.EnrollmentType == type.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetWaitlistAsync(Guid courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.AccountHolder)
            .Where(e => e.CourseId == courseId && e.EnrollmentType == EnrollmentType.Waitlisted)
            .OrderBy(e => e.WaitlistPosition)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Enrollments.AnyAsync(e => e.Id == id);
    }
}
