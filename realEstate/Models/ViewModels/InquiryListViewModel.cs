using System;

namespace realEstate.Models.ViewModels
{
    public class InquiryListViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = null!;
        public string PropertyImageUrl { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;
    }
}
