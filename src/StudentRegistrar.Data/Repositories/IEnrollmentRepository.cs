using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId, Guid? semesterId = null);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
    Task<IEnumerable<Enrollment>> GetBySemesterAsync(Guid semesterId);
    Task<IEnumerable<Enrollment>> GetAllAsync();
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Enrollment>> GetByTypeAsync(Guid courseId, EnrollmentType type);
    Task<int> GetEnrollmentCountAsync(Guid courseId, EnrollmentType type);
    Task<int> GetNextWaitlistPositionAsync(Guid courseId);
    Task<bool> HasEnrollmentAsync(Guid studentId, Guid courseId, EnrollmentType? type = null);
    Task<IEnumerable<Enrollment>> GetWaitlistAsync(Guid courseId);
    Task<bool> ExistsAsync(Guid id);
}
