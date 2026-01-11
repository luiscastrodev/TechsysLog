using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOS;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<BusinessResult<AuthenticationResponseDTO>> LoginAsync(string login, string password);
        Task LockUserAsync(Guid userId, DateTime lockoutEnd);
        Task UnlockUserAsync(Guid userId);
        Task RegisterFailedLoginAsync(User user, int maxAttempts, TimeSpan lockoutDuration);
        Task<BusinessResult<bool>> Logout(string refreshToken);
        Task<BusinessResult<AuthenticationResponseDTO>> RefreshToken(string token);

    }
}
