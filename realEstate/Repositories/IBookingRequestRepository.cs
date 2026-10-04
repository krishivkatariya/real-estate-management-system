using System.Collections.Generic;
using System.Threading.Tasks;
using realEstate.Models;

namespace realEstate.Repositories
{
    public interface IBookingRequestRepository
    {
        Task AddAsync(BookingRequest request);
        Task<BookingRequest?> GetByIdAsync(int id);
        Task<IEnumerable<BookingRequest>> GetByBuyerAsync(string buyerId);
        Task<IEnumerable<BookingRequest>> GetBySellerAsync(string sellerId);
        Task UpdateAsync(BookingRequest request);
    }
}
