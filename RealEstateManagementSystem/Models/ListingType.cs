namespace RealEstateManagementSystem.Models
{
    /// <summary>
    /// Describes how a property is being offered by its owner.
    /// Stored as an int in the database so the values stay stable if the enum is re-ordered.
    /// </summary>
    public enum ListingType
    {
        /// <summary>Property is offered for outright sale.</summary>
        Sale = 1,

        /// <summary>Property is offered on rent / lease.</summary>
        Rent = 2
    }
}
