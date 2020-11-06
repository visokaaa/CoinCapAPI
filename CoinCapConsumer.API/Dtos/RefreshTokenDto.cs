using System.ComponentModel.DataAnnotations;

namespace CoinCapConsumer.API.Dtos
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}