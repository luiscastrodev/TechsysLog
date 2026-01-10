using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Services
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthenticationService(IUserRepository userRepository, IPasswordHasherService passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<BusinessResult<AuthenticationResponseDTO>> LoginAsync(string login, string password)
        {
            var user = await _userRepository.GetByEmailAsync(login);

            if (user == null)
                throw new BusinessException("Login ou senha inválidos.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                throw new BusinessException($"Conta bloqueada até {user.LockoutEnd.Value:dd/MM/yyyy HH:mm}.");

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                await RegisterFailedLoginAsync(user, maxAttempts: 5, lockoutDuration: TimeSpan.FromMinutes(15));
                throw new BusinessException("Login ou senha inválidos.");
            }


            var tokenData = _tokenService.GenerateToken(user);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenData.RefreshToken,
                CreatedAt = DateTime.Now,
                ExpiresAt = tokenData.RefreshTokenExpiresAt
            };

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _userRepository.UpdateAsync(user);

            return Success(tokenData);
        }


        public async Task LockUserAsync(Guid userId, DateTime lockoutEnd)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new BusinessException("Usuário não encontrado");

            user.LockoutEnd = lockoutEnd;
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnlockUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new BusinessException("Usuário não encontrado");

            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);

        }

        public async Task RegisterFailedLoginAsync(User user, int maxAttempts, TimeSpan lockoutDuration)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= maxAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
                user.FailedLoginAttempts = 0;
            }

            await _userRepository.UpdateAsync(user);

        }
    }

}
