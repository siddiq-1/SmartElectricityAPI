using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SmartElectricityAPI.Enums;
using System.ComponentModel;

namespace SmartElectricityAPI.Models;

public class MqttMessageLog : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Inverter Inverter { get; set; }
    public int InverterId { get; set; }
    public Direction Direction { get; set; }
    public MqttMessageOrigin MqttMessageOrigin { get; set; }
    public MQttMessageType MQttMessageType { get; set; }
    public string Topic { get; set; }
    public string Payload { get; set; }
    public ActionTypeCommand ActionTypeCommand { get; set; }
    [DefaultValue(true)]
    public bool CommandDispatched { get; set; }
    public string ExtraInfo { get; set; }

}
