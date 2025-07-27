using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<IEnumerable<Student>> GetByAccountHolderIdAsync(Guid accountHolderId);
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Student>> GetStudentsWithEnrollmentsAsync(Guid? semesterId = null);
    Task<IEnumerable<Student>> GetByAccountHolderAsync(Guid accountHolderId);
    Task<bool> ExistsAsync(Guid id);
}
