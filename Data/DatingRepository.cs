using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var maxAge = DateTime.Today.AddYears(-userParams.MinAge);
            var minAge = DateTime.Today.AddYears(-userParams.MaxAge - 1);

            var users = _context.Users
                .Where(u => u.DateOfBirth >= minAge && u.DateOfBirth <= maxAge)
                .Where(u => u.Id != userParams.UserId);

            if (!string.IsNullOrEmpty(userParams.Gender))
            {
                users = users.Where(u => u.Gender.ToLower().Equals(userParams.Gender.ToLower()));
            }

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, true);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, false);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            users = users.Include(u => u.Photos);
            users = users.AsQueryable();

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                .Include(u => u.Likees)
                .Include(u => u.Likeers)
                .FirstOrDefaultAsync(u => u.Id == id);

            return likers ? user.Likeers.Where(u => u.LikeeId == id).Select(u => u.LikerId) :
                    user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Photo> GetMainPhoto(int id)
        {
            return await _context.Photos.Where(p => p.UserId == id).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Like> GetLike(int userId, int recipentId)
        {
            return await _context.Likes.FirstOrDefaultAsync(l => l.LikeeId == recipentId && l.LikerId == userId);
        }
    }
}
