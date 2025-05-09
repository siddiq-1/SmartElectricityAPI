namespace SmartElectricityAPI.Attributes
{
    public class ModelAttribute : Attribute
    {
        public string Description { get; set; }

        public ModelAttribute(string description)
        {
            Description = description;
        }
    }
}
