using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
        Task Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();
        Task<User> GetUser(int id);
        Task<PagedList<User>> GetUsers(UserParams userParams);
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhoto(int id);
        Task<Like> GetLike(int userId, int recipentId);
        Task<Message> GetMessage(int id);
        Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);
        public Task<IEnumerable<Message>> GetMessageThread(int userId, int recipentId);
    }
}
