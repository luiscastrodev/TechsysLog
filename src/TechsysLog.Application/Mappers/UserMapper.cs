using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Domain.Entities.Enums;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Mappers
{
    public static class UserMapper
    {
        public static UserResponseDto ToDto(this User user)
        {
            if (user == null) return null!; 

            return new UserResponseDto(
                Id: user.Id,
                Name: user.Name,
                Email: user.Email,
                Active: user.Active
            );
        }
        public static User ToEntity(this UserResponseDto dto, string passwordHash, UserRole role = UserRole.User)
        {
            if (dto == null) return null!;

            return new User(
                name: dto.Name,
                email: dto.Email,
                password: passwordHash,
                userRole: role
            );
        }
    }

}
