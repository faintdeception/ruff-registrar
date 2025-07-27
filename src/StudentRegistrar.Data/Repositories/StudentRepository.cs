using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly StudentRegistrarDbContext _context;

    public StudentRepository(StudentRegistrarDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _context.Students
            .Include(s => s.AccountHolder)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Semester)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Student>> GetByAccountHolderIdAsync(Guid accountHolderId)
    {
        return await _context.Students
            .Include(s => s.AccountHolder)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Where(s => s.AccountHolderId == accountHolderId)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
            .Include(s => s.AccountHolder)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<Student> CreateAsync(Student student)
    {
        student.CreatedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(student.Id) ?? student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        student.UpdatedAt = DateTime.UtcNow;
        
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(student.Id) ?? student;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<Student>> GetStudentsWithEnrollmentsAsync(Guid? semesterId = null)
    {
        var query = _context.Students
            .Include(s => s.AccountHolder)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Semester)
            .Where(s => s.Enrollments.Any());

        if (semesterId.HasValue)
        {
            query = query.Where(s => s.Enrollments.Any(e => e.SemesterId == semesterId.Value));
        }

        return await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Students.AnyAsync(s => s.Id == id);
    }
}
