using System.ComponentModel.DataAnnotations;

namespace HireFlow.DTOs.Application
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";
    }
}