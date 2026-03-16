using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    public interface IUserlogRepository
    {
        Task<Userlog?> GetByIdAsync(int id);
        Task<IEnumerable<Userlog>> GetAllAsync();
        Task<Userlog> AddAsync(Userlog userlog);
        Task<Userlog> UpdateAsync(Userlog userlog);
        Task<bool> DeleteAsync(int id);
    }
}
