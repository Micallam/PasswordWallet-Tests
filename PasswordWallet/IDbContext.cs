﻿using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet
{
    public interface IDbContext
    {
        int CreateUser(UserModel user);

        List<UserModel> GetUsersList();

        UserModel GetUserById(int idUser);

        UserModel GetUserByLogin(string login);

        int UpdateUser(UserModel userToUpdate);

        int DeleteUser(int userId);

        List<PasswordModel> GetPasswordListByUserId(int userId);

        int UpdatePasswordHash(int userId, string oldPassword, string newPassword);

        int CreatePassword(PasswordModel password);

        PasswordModel GetPasswordByHash(string passwordHash);

        int DeletePassword(string passwordHash);

        int LogUserLogin(int userId, LoginStatus loginStatus, string ipAddress);

        List<LoginLog> GetLoginLogListByUserId(int userId);

        int SaveUserBlocade(LoginBlocade loginBlocade);

        int UpdateUserBlocade(LoginBlocade loginBlocade);

        int DeleteUserBlocade(int userId);

        int SaveIpBlocade(LoginBlocade loginBlocade);

        int UpdateIpBlocade(LoginBlocade loginBlocade);

        int DeleteIpBlocade(string ipAddress);

        LoginBlocade GetBlocadeByUserId(int userId);

        LoginBlocade GetBlocadeByIp(string ipAddress);

        LoginBlocade GetActiveBlocadeByUserId(int userId);

        LoginBlocade GetActiveBlocadeByIp(string ipAddress);
    }
}
