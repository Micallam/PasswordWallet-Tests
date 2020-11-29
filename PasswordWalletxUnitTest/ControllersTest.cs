using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using PasswordWallet;
using PasswordWallet.Controllers;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xunit;

namespace PasswordWalletxUnitTest
{
    public class ControllersTest
    {
        [Theory]
        [InlineData("abcd", "2AIvIGCtbv0perc9zFNVybIUBUsNF3ahNqZp0mp9OxT3OqDQ6/8Z7jMzaPAWS2QZqW2knj5IF1Pn6Wtxa9zLbw==")]
        [InlineData("Test Value", "Cx+wHrgzb37/CH91tQeBXIi2Gyvaaiop5+zoIk0KjcBbhRsqJMa0d5HRvgu5Eha9xiBtl6/FbYJkJ1/RcQ59hw==")]
        [InlineData("Sp3c!@l", "z865UvEVxtJnA5viRVknZmJVnkNE65V2u20Cb4PmMTE4OjHASFdnmTdw6SjJF+mHnyao3994EC4H9pXYKU35nw==")]
        public void TestCalculateSHA512_GeneratesExpectedResult_WhenStringValueIsPassed(string textToHash, string expected)
        {
            string actual = UsersController.CalculateSHA512(textToHash);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("abcd", "key", "TI5asOvyi9fVTYb86xwV42UXuhUi07MEhdivFII4dxxeBGghxFejmduck2oeK5wvNwaIztmWjnimp8pnjm0+hA==")]
        public void TestCalculateHMAC_GeneratesExpectedResult_WhenStringValueIsPassed(string textToHash, string key, string expected)
        {
            string actual = UsersController.CalculateHMAC(textToHash, key);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(10, "login", "password", "salt", true)]
        [InlineData(1, "LoggedUser", "P@ss^!@#", "qwe123!@#", false)]
        public void TestCheckLoginCridentials_ReturnsLoggedUserId_WhenCorrectCridentialsAreGiven(
            int userId,
            string userLogin,
            string userPassword,
            string userSalt,
            bool isPasswordKeptAsHash)
        {
            UserModel user = new UserModel
            {
                Id = userId,
                IsPasswordKeptAsHash = isPasswordKeptAsHash,
                Login = userLogin,
                PasswordHash = isPasswordKeptAsHash ? 
                    UsersController.GetPasswordHashSHA512(userPassword, userSalt) : 
                    UsersController.GetPasswordHMAC(userPassword, userSalt),
                Salt = userSalt
            };

            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetUserByLogin(It.IsAny<string>()))
                .Returns(user);

            mockDbContext
                .Setup(m => m.GetActiveBlocadeByUserId(It.IsAny<int>()))
                .Returns(null as LoginBlocade);

            mockDbContext
                .Setup(m => m.GetActiveBlocadeByIp(It.IsAny<string>()))
                .Returns(null as LoginBlocade);

            UserLogin loginCridentials = new UserLogin()
            {
                Login = userLogin,
                Password = userPassword
            };

            UsersController usersController = new UsersController(dbContext: mockDbContext.Object);

            Assert.Equal(
                userId,
                usersController.CheckLoginCridentials(loginCridentials));
        }

        [Fact]
        public void TestCreateUser_ThrowsException_IfDuplicatedLoginIsPassed()
        {
            string userLogin = "login";

            UserModel user = new UserModel
            {
                Login = userLogin,
                PasswordHash = "hash",
                IsPasswordKeptAsHash = false,
                Salt = "salt"
            };

            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetUserByLogin(userLogin))
                .Returns(user);

            UsersController usersController = new UsersController(dbContext: mockDbContext.Object);

            Assert.Throws<DuplicateNameException>(
                () => usersController.CreateUser(user)
                );
        }

        [Theory]
        [InlineData(1, "", "login")]
        [InlineData(0, "hash", "")]
        [InlineData(0, "", "")]
        public void TestCreatePassword_ThrowsException_IfNullArgumentIsPassed(int userId, string passwordHash, string login)
        {
            PasswordModel password = new PasswordModel
            {
                Login = login,
                PasswordHash = passwordHash,
                IdUser = userId
            };

            PasswordsController usersController = new PasswordsController();

            Assert.Throws<ArgumentNullException>(
                () => usersController.CreatePassword(password)
                );
        }

        [Fact]
        public void TestLogUserLogin_ThrowsException_WhenUnknownUserIdIsGiven()
        {
            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetUserByLogin(It.IsAny<string>()))
                .Returns(null as UserModel);

            UsersController usersController = new UsersController(dbContext: mockDbContext.Object);

            Assert.Throws<UnknownLoginException>(
                () => usersController.LogUserLogin("login", LoginStatus.Failed));
        }

        [Fact]
        public void TestDeleteIpBlocadeIfExists_ReturnsOne_WhenBlocadeExists()
        {
            string ipToTest = "1:1:1:1";

            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetBlocadeByIp(ipToTest))
                .Returns(new LoginBlocade());

            mockDbContext
                .Setup(m => m.DeleteIpBlocade(ipToTest))
                .Returns(1);

            UsersController usersController = new UsersController(dbContext: mockDbContext.Object);

            int expected = 1;
            int actual = usersController.DeleteIpBlocadeIfExists(ipToTest);

            Assert.Equal(expected, actual);
        }
        [Fact]
        public void TestDeleteIpBlocadeIfExists_ReturnsZero_WhenBlocadeDoesntExists()
        {
            string ipToTest = "1:1:1:1";

            Mock<IDbContext> mockDbContext = new Mock<IDbContext>();
            mockDbContext
                .Setup(m => m.GetBlocadeByIp(ipToTest))
                .Returns(null as LoginBlocade);

            mockDbContext
                .Setup(m => m.DeleteIpBlocade(ipToTest))
                .Returns(0);

            UsersController usersController = new UsersController(dbContext: mockDbContext.Object);

            int expected = 0;
            int actual = usersController.DeleteIpBlocadeIfExists(ipToTest);

            Assert.Equal(expected, actual);
        }
    }
}
