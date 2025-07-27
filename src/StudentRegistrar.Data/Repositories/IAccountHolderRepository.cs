using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface IAccountHolderRepository
{
    Task<AccountHolder?> GetByIdAsync(Guid id);
    Task<AccountHolder?> GetByKeycloakUserIdAsync(string keycloakUserId);
    Task<AccountHolder?> GetByEmailAsync(string email);
    Task<IEnumerable<AccountHolder>> GetAllAsync();
    Task<AccountHolder> CreateAsync(AccountHolder accountHolder);
    Task<AccountHolder> UpdateAsync(AccountHolder accountHolder);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<AccountHolder>> GetAccountHoldersWithStudentsAsync(Guid? semesterId = null);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}
