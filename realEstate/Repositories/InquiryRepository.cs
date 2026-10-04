using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Repositories
{
    public class InquiryRepository : IInquiryRepository
    {
        private readonly ApplicationDbContext _db;

        public InquiryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Inquiry inquiry)
        {
            _db.Inquiries.Add(inquiry);
            await _db.SaveChangesAsync();
        }

        public async Task<Inquiry> GetByIdAsync(int id)
        {
            return await _db.Inquiries
                .Include(i => i.Property)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Inquiry>> GetByUserAsync(string userId)
        {
            return await _db.Inquiries
                .Include(i => i.Property)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inquiry>> GetByOwnerAsync(string ownerId)
        {
            return await _db.Inquiries
                .Include(i => i.Property)
                .Include(i => i.User)
                .Where(i => i.Property.OwnerId == ownerId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Inquiry inquiry)
        {
            _db.Inquiries.Update(inquiry);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsRecentDuplicateAsync(string userId, int propertyId, string subject)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            return await _db.Inquiries.AnyAsync(i => i.UserId == userId && i.PropertyId == propertyId && i.Subject == subject && i.CreatedAt >= cutoff);
        }
    }
}
