namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Backing model for /Properties/Details/{id}.
    /// Only safe, non-sensitive owner information is exposed to the screen.
    /// </summary>
    public class PropertyDetailsViewModel
    {
        public required Property Property { get; set; }

        /// <summary>Display name of the owner. No email, password or other security information is shown.</summary>
        public string OwnerName { get; set; } = "Unknown";

        /// <summary>True when the current visitor is the owner of the listing or an Administrator.</summary>
        public bool CanManage { get; set; }

        /// <summary>True when the listing is not publicly visible yet (Pending / Rejected / Inactive).</summary>
        public bool IsRestrictedListing { get; set; }
    }
}
