using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Grpc.Server.Services
{
    public class TokenModel{
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public bool Success { get; set; }
    }

    public class UserModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    
    public class JwtTokenValidationService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public JwtTokenValidationService(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public async Task<TokenModel> GenerateTokenAsync(UserModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserName,model.Password,false,false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sid,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName),
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("EE196CF5-B79B-40D7-985F-D71198E892E9"));

                var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    "localhost",
                    "localhost",
                    claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds);

                return new TokenModel
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration=token.ValidTo,
                    Success =true
            };
        }

            return new TokenModel
            {
                Success = false
            };
        }
    }
}
