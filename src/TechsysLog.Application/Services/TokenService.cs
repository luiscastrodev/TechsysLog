using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Services
{
    public class TokenService : ITokenService
    {

        private readonly IConfiguration _configuration;

        private const int _TOKEN_EXPIRATION = 3;
        private const int _REFRESH_TOKEN_EXPIRATION_DAYS = 7;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthenticationResponseDTO GenerateToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            ArgumentNullException.ThrowIfNull(jwtKey);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var accessTokenExpiration = DateTime.UtcNow.AddHours(_TOKEN_EXPIRATION);

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_REFRESH_TOKEN_EXPIRATION_DAYS);

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())

                }),
                Expires = accessTokenExpiration,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encodedToken = tokenHandler.WriteToken(token);

            return new AuthenticationResponseDTO
            {
                UserId = user.Id,
                AccessToken = encodedToken,
                RefreshTokenExpiresAt = refreshTokenExpiration,
                RefreshToken = GenerateRefreshToken()

            };
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var token = Convert.ToBase64String(randomNumber);

            return token;
        }
    }
}
