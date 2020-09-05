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

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public async Task Add<T>(T entity) where T : class
        {
            await _context.AddAsync(entity);
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
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
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

            //users = users.Include(u => u.Photos);
            users = users.AsQueryable();

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            return likers
                ? user.Likeers.Where(u => u.LikeeId == id).Select(u => u.LikerId)
                : user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
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

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages
                        .Where(u => u.RecipientId == messageParams.UserId
                                    && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages
                        .Where(u => u.SenderId == messageParams.UserId
                                    && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages
                        .Where(u => u.RecipientId == messageParams.UserId
                                    && u.IsRead == false
                                    && u.RecipientDeleted == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipentId)
        {
            var messages = await _context.Messages
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false
                                                    && m.SenderId == recipentId
                            || m.RecipientId == recipentId && m.SenderId == userId
                                                           && m.SenderDeleted == false)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            return messages;
        }
    }
}
