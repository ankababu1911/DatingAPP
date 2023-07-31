using System.Text;
using System.Security.Cryptography;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DTOS;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext dataContext,ITokenService tokenService)
        {
            _tokenService = tokenService;
            _dataContext = dataContext;
            
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExist(registerDto.Username)) return BadRequest("user name already exists");
            using var hmac=new HMACSHA512();
            var user=new AppUser{
                UserName=registerDto.Username.ToLower(),
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt=hmac.Key
            };

            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return new UserDto{
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }
         [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user= await _dataContext.Users.SingleOrDefaultAsync(x=>x.UserName==loginDto.Username);

            if(user==null) return Unauthorized("invalid username");
            using var hmac=new HMACSHA512(user.PasswordSalt);
            var ComputedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for(int i=0; i<ComputedHash.Length;i++)
            {
                if(ComputedHash[i]!=user.PasswordHash[i]) return Unauthorized("invalid password");
            }
              return new UserDto{
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExist(string username)
        {
            return await _dataContext.Users.AnyAsync(x=>x.UserName.ToLower()==username.ToLower());
        }
    }
}