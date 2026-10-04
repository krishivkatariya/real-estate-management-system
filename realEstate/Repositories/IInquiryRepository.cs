using System.Collections.Generic;
using System.Threading.Tasks;
using realEstate.Models;

namespace realEstate.Repositories
{
    public interface IInquiryRepository
    {
        Task<Inquiry> GetByIdAsync(int id);
        Task<IEnumerable<Inquiry>> GetByUserAsync(string userId);
        Task<IEnumerable<Inquiry>> GetByOwnerAsync(string ownerId);
        Task AddAsync(Inquiry inquiry);
        Task UpdateAsync(Inquiry inquiry);
        Task<bool> ExistsRecentDuplicateAsync(string userId, int propertyId, string subject);
    }
}
