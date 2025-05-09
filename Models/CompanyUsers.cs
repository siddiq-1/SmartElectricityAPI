using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SmartElectricityAPI.Models
{
    public class CompanyUsers : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public int UserId { get; set; }

        public User? User { get; set; }
        public int? PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
