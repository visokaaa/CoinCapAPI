using System;
using System.Threading.Tasks;
using CoinCapConsumer.API.Models;

namespace CoinCapConsumer.API.Data
{
    public interface IApplicationRepository
    {
        void Add<T>(T entity) where T: class;
        T Find<T>(T entity, string id) where T : class;
        void Delete<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        Task<bool> SaveAll();
        Task<UserFavoriteCoin> FindCoinById(User user, string coinId);
        Task<string> FindCoinsByUser(User user);
        Task<User> FindUserByRefreshToken(string refreshToken);
    }
}