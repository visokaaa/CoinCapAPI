using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CoinCapConsumer.API.Data;
using CoinCapConsumer.API.Dtos;
using CoinCapConsumer.API.Models;
using CoinTask.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoinCapConsumer.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public readonly IConfiguration _config;
        private readonly IApplicationRepository _applicationRepository;
        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config, IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {


            var userToCreate = new User
            {
                FirstName = userForRegisterDto.Firstname,
                LastName = userForRegisterDto.Lastname,
                Email = userForRegisterDto.Email,
                UserName = userForRegisterDto.Email
            };

            var userCheck = await _userManager.FindByEmailAsync(userToCreate.Email.ToUpper());
            if (userCheck != null)
            {
                return BadRequest("Perdoruesi me kete email ekziston!");
            }

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);
            if (result.Succeeded)
            {
                return Ok(new { name = userToCreate.FirstName, Email = userToCreate.Email, message = "Registration successful" });
            }

            return BadRequest(result.Errors);

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var user = await _userManager.FindByEmailAsync(userForLoginDto.Email);
            if (user == null)
            {
                return BadRequest("Email or password is incorrect!");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if (result.Succeeded)
            {
                if (user.RefreshToken == null || !ValidateRefreshToken(user.RefreshTokenExpireTime))
                {
                    user.RefreshToken = GenerateRefreshToken();
                    user.RefreshTokenExpireTime = DateTime.Now.AddMinutes(30);

                    _applicationRepository.Update(user);
                    await _applicationRepository.SaveAll();
                }
                var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == userForLoginDto.Email.ToUpper());
                return Ok(new
                {
                    token = GenerateJWTToken(appUser),
                    refreshToken = user.RefreshToken,
                    user = userForLoginDto,
                    message = "Logged In successfully"
                });
            }

            return BadRequest("Could not log in!");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var user = await _applicationRepository.FindUserByRefreshToken(refreshTokenDto.RefreshToken);
            if (user == null)
                return BadRequest("Couldn't find a user with that refresh token");
            
            if (!ValidateRefreshToken(user.RefreshTokenExpireTime))
                return BadRequest("Your refresh token has expired");
            
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpireTime = DateTime.Now.AddMinutes(10);

            _applicationRepository.Update(user);
            await _applicationRepository.SaveAll();

            var token = GenerateJWTToken(user);

            return Ok(new {
                token = token,
                refreshToken = user.RefreshToken
            });
        }

        public bool ValidateRefreshToken(DateTime refreshTokenExpireTime)
        {
            return refreshTokenExpireTime < DateTime.Now ? false : true;
        }

        public string GenerateRefreshToken()    
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GenerateJWTToken(User user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                new Claim(ClaimTypes.Name, user.UserName),

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}