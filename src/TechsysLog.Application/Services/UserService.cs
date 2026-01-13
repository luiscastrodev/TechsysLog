using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Mappers;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BusinessResult<IEnumerable<UserResponseDto>>> GetAllAsync()
        {
            var users =  await _userRepository.GetAllAsync();
            var userDtos = users.Select(user => user.ToDto());

            return Success(userDtos.AsEnumerable());
        }

        public async Task<BusinessResult<UserResponseDto>> GetById(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) return BusinessResult<UserResponseDto>.Failure("Não encontrado.");

            return Success(user.ToDto());
        }

        public async Task<BusinessResult<UserResponseDto>> RegisterAsync(CreateUserDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new BusinessException("E-mail já cadastrado.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User(dto.Name, dto.Email, passwordHash, dto.Role);
            await _userRepository.AddAsync(user);

            return Success(user.ToDto());
        }
    }
}
