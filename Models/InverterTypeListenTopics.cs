using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class InverterTypeListenTopics :BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public InverterType InverterType { get; set; }
    public int InverterTypeId { get; set; }
    public string TopicName { get; set; }

}
