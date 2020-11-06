using System.ComponentModel.DataAnnotations;

namespace CoinTask.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 8 characters")]
        public string  Password { get; set; }
    }
}