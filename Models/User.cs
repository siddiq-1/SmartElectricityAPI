
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace SmartElectricityAPI.Models;

[Microsoft.EntityFrameworkCore.Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(255)]
    public string Username { get; set; }
    [Required]
    [MaxLength(255)]
    //[JsonIgnore]
    public string? Password { get; set; }
    [Required,
    MaxLength(255), EmailAddress]
    public string Email { get; set; }
    /*
    public int? PermissionId { get; set; }
    [ValidateNever]
    public Permission? Permission { get; set; }
    */
    public int? SelectedCompanyId { get; set; }
    public Company? Company { get; set; }
    public string? RefreshToken { get; set; }
    public string? ClientId { get; set; }
    public bool IsAdmin { get; set; }
}
