using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TEK.Core.UoW;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Claims;
using Stories.Models;
using Stories.VM.Request;

namespace Stories.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateUser(LogInRequest login);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private IConfiguration _config;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<User> AuthenticateUser(LogInRequest login)
        {
            var user = await _unitOfWork.GetRepository<User>().FindAsync(x => x.Username == login.Username && x.Password == login.Password);

            return user;
        }

        private string GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim("Username", userInfo.Username),
                new Claim("IsAuthor", userInfo.IsAuthor.ToString()),
                new Claim("Name", userInfo.Name)
            };

            var token = new JwtSecurityToken(_config["AppSetting:Issuer"],
                _config["AppSetting:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
