namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>One row of the Admin Property Types table: the type plus how many listings use it.</summary>
    public class PropertyTypeListItemViewModel
    {
        public required PropertyType PropertyType { get; set; }

        /// <summary>Number of property listings currently using this type.</summary>
        public int PropertyCount { get; set; }
    }
}
