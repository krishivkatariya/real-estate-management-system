namespace RealEstateManagementSystem.Models
{
    /// <summary>
    /// Lifecycle status of a property listing. Only <see cref="Approved"/> properties are
    /// shown in the public listing; <see cref="Pending"/> and <see cref="Rejected"/> are
    /// only visible to the owner and to Administrators.
    /// </summary>
    public enum PropertyStatus
    {
        /// <summary>Newly created or re-submitted listing waiting for Admin review.</summary>
        Pending = 1,

        /// <summary>Approved by an Admin and visible in the public listing.</summary>
        Approved = 2,

        /// <summary>Rejected by an Admin. A rejection reason is stored on the property.</summary>
        Rejected = 3,

        /// <summary>Reserved for a future phase (buy transaction).</summary>
        Sold = 4,

        /// <summary>Reserved for a future phase (rent transaction).</summary>
        Rented = 5,

        /// <summary>Temporarily taken off the market by the owner.</summary>
        Inactive = 6
    }
}
