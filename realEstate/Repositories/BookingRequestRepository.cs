using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Repositories
{
    public class BookingRequestRepository : IBookingRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(BookingRequest request)
        {
            await _db.BookingRequests.AddAsync(request);
        }

        public async Task<BookingRequest?> GetByIdAsync(int id)
        {
            return await _db.BookingRequests.Include(b => b.Property).Include(b => b.Buyer).Include(b => b.Seller).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<BookingRequest>> GetByBuyerAsync(string buyerId)
        {
            return await _db.BookingRequests.Where(b => b.BuyerId == buyerId).Include(b => b.Property).Include(b => b.Seller).OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<BookingRequest>> GetBySellerAsync(string sellerId)
        {
            return await _db.BookingRequests.Where(b => b.SellerId == sellerId).Include(b => b.Property).Include(b => b.Buyer).OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        public async Task UpdateAsync(BookingRequest request)
        {
            _db.BookingRequests.Update(request);
        }
    }
}
