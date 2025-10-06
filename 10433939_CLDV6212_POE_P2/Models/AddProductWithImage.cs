using System.ComponentModel.DataAnnotations;

namespace _10433939_CLDV6212_POE_P2.Models
{
    public class AddProductWithImage
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "Profile Picture")]
        public IFormFile? Image { get; set; }
    }
}
