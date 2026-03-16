using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    public interface ISessionRepository
    {
        Task<Session?> GetByIdAsync(int id);
        Task<IEnumerable<Session>> GetAllAsync();
        Task<Session> AddAsync(Session session);
        Task<Session> UpdateAsync(Session session);
        Task<bool> DeleteAsync(int id);
    }
}
