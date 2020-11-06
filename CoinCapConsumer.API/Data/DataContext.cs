using CoinCapConsumer.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoinCapConsumer.API.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<UserFavoriteCoin> UserFavoriteCoins { get; set; }
    }
}