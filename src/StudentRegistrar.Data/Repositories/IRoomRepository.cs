using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
    Task<Room?> GetByNameAsync(string name);
    Task<IEnumerable<Room>> GetAllAsync();
    Task<IEnumerable<Room>> GetByTypeAsync(RoomType roomType);
    Task<Room> CreateAsync(Room room);
    Task<Room> UpdateAsync(Room room);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<bool> IsRoomInUseAsync(Guid roomId);
}
