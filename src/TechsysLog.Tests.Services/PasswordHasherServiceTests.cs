using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.Services;
using Xunit;

namespace TechsysLog.Tests.Services
{
    public class PasswordHasherServiceTests
    {
        private readonly PasswordHasherService _sut;

        public PasswordHasherServiceTests()
        {
            _sut = new PasswordHasherService();
        }

        [Fact]
        public void HashPasswordShouldReturnDifferentHashEachTime()
        {
            var password = "senhaSegura123";
            var hash1 = _sut.HashPassword(password);
            var hash2 = _sut.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPasswordShouldReturnTrueForCorrectPassword()
        {
            var password = "senhaSegura123";
            var hash = _sut.HashPassword(password);

            var result = _sut.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPasswordShouldReturnFalseForIncorrectPassword()
        {
            var password = "senhaSegura123";
            var wrongPassword = "senhaErrada456";
            var hash = _sut.HashPassword(password);

            var result = _sut.VerifyPassword(wrongPassword, hash);

            Assert.False(result);
        }

        [Fact]
        public void VerifyPasswordShouldReturnFalseForMalformedHash()
        {
            var password = "senhaSegura123";
            var malformedHash = "isso_nao_eh_um_hash_valido";

            var result = _sut.VerifyPassword(password, malformedHash);

            Assert.False(result);
        }
    }
}
