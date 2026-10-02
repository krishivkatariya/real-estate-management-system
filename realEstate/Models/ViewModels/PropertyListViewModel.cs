using System.Collections.Generic;

namespace realEstate.Models.ViewModels;

public class PropertyListViewModel
{
    public IEnumerable<Property> Properties { get; set; } = Enumerable.Empty<Property>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    // filters
    public string? Search { get; set; }
    public string? City { get; set; }
    public realEstate.Models.PropertyType? PropertyType { get; set; }
    public realEstate.Models.ListingPurpose? ListingPurpose { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public realEstate.Models.PropertyStatus? Status { get; set; }
    public string? Sort { get; set; }
    public IEnumerable<int> FavoritedPropertyIds { get; set; } = Enumerable.Empty<int>();
}
