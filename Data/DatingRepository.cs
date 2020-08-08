using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        private IDatingRepository _datingRepositoryImplementation;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User> GetUser(int id)
        {
            return await _context.Users.Include(user => user.Photos).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users
                .Include(user => user.Photos)
                .ToListAsync();
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Photo> GetMainPhoto(int id)
        {
            return await _context.Photos.Where(p => p.UserId == id).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}
