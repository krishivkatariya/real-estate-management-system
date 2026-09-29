namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>One row of the Admin Locations table: the location plus how many listings use it.</summary>
    public class LocationListItemViewModel
    {
        public required Location Location { get; set; }

        /// <summary>Number of property listings currently using this location.</summary>
        public int PropertyCount { get; set; }
    }
}
