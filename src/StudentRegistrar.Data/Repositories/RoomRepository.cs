using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly StudentRegistrarDbContext _context;

    public RoomRepository(StudentRegistrarDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id)
    {
        return await _context.Rooms
            .Include(r => r.Courses)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> GetByNameAsync(string name)
    {
        return await _context.Rooms
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        return await _context.Rooms
            .Include(r => r.Courses)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetByTypeAsync(RoomType roomType)
    {
        return await _context.Rooms
            .Where(r => r.RoomType == roomType)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(int minCapacity)
    {
        return await _context.Rooms
            .Where(r => r.Capacity >= minCapacity)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Room> CreateAsync(Room room)
    {
        room.CreatedAt = DateTime.UtcNow;
        room.UpdatedAt = DateTime.UtcNow;
        
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        room.UpdatedAt = DateTime.UtcNow;
        
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return false;

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Rooms.AnyAsync(r => r.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _context.Rooms.Where(r => r.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<bool> IsRoomInUseAsync(Guid roomId)
    {
        return await _context.Courses.AnyAsync(c => c.RoomId == roomId);
    }
}
