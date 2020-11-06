using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CoinCapConsumer.API.Data;
using CoinCapConsumer.API.Dtos;
using CoinCapConsumer.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CoinCapConsumer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IConfiguration _configuration;
        public ApplicationController(IApplicationRepository applicationRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _applicationRepository = applicationRepository;
        }

        [HttpGet]
        public IActionResult GetCoins()
        {
            string url = _configuration.GetSection("AppSettings:CoinCapAPIBaseURL").Value;
            var jsonObject = fetchDataFromCoinApi(url);
            return Ok(jsonObject.Data);
        }

        [HttpPost("mark-unmark-coin")]
        public async Task<IActionResult> MarkUnmarkCoin([FromBody] UserCoinDto userCoinDto)
        {
            var user =  _applicationRepository.Find(new User(), userCoinDto.UserId);
            if (user == null) 
            {
                return BadRequest("Can not find user");
            }
            UserFavoriteCoin userCoin = await _applicationRepository.FindCoinById(user, userCoinDto.CoinId);
            if(userCoin == null) 
            {
                userCoin = new UserFavoriteCoin {
                    User = user,
                    CoinId = userCoinDto.CoinId
                };
                
                _applicationRepository.Add(userCoin);
            } else {
                _applicationRepository.Delete(userCoin);
            }

            await _applicationRepository.SaveAll();
            return Ok(new {
                userId = userCoin.User.FirstName,
                coinId = userCoin.CoinId
            });
        }

        [HttpGet("users/{id}/coins")]
        public async Task<IActionResult> GetUserCoinCaps(string id)
        {
            var user = _applicationRepository.Find(new User(), id);
            if (user == null) 
                return BadRequest("Could not find user");
            
            var coins = await _applicationRepository.FindCoinsByUser(user);

            if (coins.Length == 0) 
                return BadRequest("This user has no favorite coins");

            string url = _configuration.GetSection("AppSettings:CoinCapAPIBaseURL").Value + "?ids=" + coins;
            var jsonObject = fetchDataFromCoinApi(url);
            return Ok(jsonObject.Data);
        }

        private CoinCapResponseDto fetchDataFromCoinApi(string url)
        {
            var coins = new List<CoinDto>();
            CoinCapResponseDto jsonObject;
            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                var json = response.Content.ReadAsStringAsync().Result;
                jsonObject = JsonConvert.DeserializeObject<CoinCapResponseDto>(json);

            }
            return jsonObject;
        }
    }
}