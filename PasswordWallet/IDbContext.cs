using PasswordWallet.Models;
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
    }
}
