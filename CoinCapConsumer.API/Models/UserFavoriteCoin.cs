namespace CoinCapConsumer.API.Models
{
    public class UserFavoriteCoin
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string CoinId { get; set; }
    }
}