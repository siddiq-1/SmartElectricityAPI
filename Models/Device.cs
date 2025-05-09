using SmartElectricityAPI.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models
{
    public class Device : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        public bool FuseboxForcedOff { get; set; }
        public bool FuseboxForcedOn { get; set; }
        public double MaxStopHoursIn24h { get; set; }
        public double MaxStopHoursConsecutive { get; set; }
        public double MaxForcedOnHoursIn24h { get; set; }
        public double ForcedOnPercentageForComingHourToEnable { get; set; }
        public bool ForcedOn { get; set; }
        public bool ForcedOff { get; set; }
        public bool MediumOn { get; set; }
        public double? TemperatureInStandardMode { get; set; }
        public double? TemperatureInForcedOnMode { get; set; }
        [Model("First hour %, that Kw price needs to be higher before heating (To compensate energy loss")]
        public double FirstHourPercentageKwPriceRequirementBeforeHeating { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public bool AutoModeEnabled { get; set; } = false;
    }
}
