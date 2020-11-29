using Moq;
using Newtonsoft.Json;
using PasswordWallet;
using PasswordWallet.Controllers;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PasswordWalletxUnitTest
{
    public class LoginLogTest
    {
        [Fact]
        public void TestCreateOrUpdateUserBlocade_ReturnsExpectedBlocadeObject_WhenUserIdIsPassed()
        {
            LoginBlocade blocadeMock = new LoginBlocade()
            {
                Id = 1,
                UserId = 1,
                IpAddress = "0.0.0.1",
                FailCount = 3,
                BlockUntil = LoginHelper.TruncateDateTime(DateTime.Now).AddSeconds(10)
            };

            LoginBlocade expectedBlocade = new LoginBlocade()
            {
                Id = 1,
                UserId = 1,
                IpAddress = "0.0.0.1",
                FailCount = 4,
                BlockUntil = LoginHelper.TruncateDateTime(DateTime.Now).AddSeconds(60)
            };

            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetBlocadeByUserId(It.IsAny<int>()))
                .Returns(blocadeMock);

            mockDbContext
                .Setup(m => m.SaveUserBlocade(It.IsAny<LoginBlocade>()))
                .Returns(0);

            mockDbContext
                .Setup(m => m.UpdateUserBlocade(It.IsAny<LoginBlocade>()))
                .Returns(0);

            LoginHelper loginHelper = new LoginHelper(mockDbContext.Object);

            var expected = JsonConvert.SerializeObject(expectedBlocade);
            var actual = JsonConvert.SerializeObject(loginHelper.CreateOrUpdateLoginUserIdBlocade(1, TimeInterval.Minutes));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestCreateOrUpdateIpBlocade_ReturnsExpectedBlocadeObject_WhenIpIsPassed()
        {
            string ipToTest = "1.1.1.1";

            LoginBlocade blocadeMock = new LoginBlocade()
            {
                Id = 1,
                UserId = 1,
                IpAddress = ipToTest,
                FailCount = 3,
                BlockUntil = LoginHelper.TruncateDateTime(DateTime.Now).AddSeconds(10)
            };

            LoginBlocade expectedBlocade = new LoginBlocade()
            {
                Id = 1,
                UserId = 1,
                IpAddress = ipToTest,
                FailCount = 4,
                BlockUntil = LoginHelper.TruncateDateTime(DateTime.Now).AddSeconds(60)
            };

            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetBlocadeByIp(ipToTest))
                .Returns(blocadeMock);

            mockDbContext
                .Setup(m => m.SaveIpBlocade(It.IsAny<LoginBlocade>()))
                .Returns(0);

            mockDbContext
                .Setup(m => m.UpdateIpBlocade(It.IsAny<LoginBlocade>()))
                .Returns(0);

            LoginHelper loginHelper = new LoginHelper(mockDbContext.Object);

            var expected = JsonConvert.SerializeObject(expectedBlocade);
            var actual = JsonConvert.SerializeObject(loginHelper.CreateOrUpdateLoginIpBlocade(ipToTest, TimeInterval.Minutes));

            Assert.Equal(expected, actual);
        }
    }
}
