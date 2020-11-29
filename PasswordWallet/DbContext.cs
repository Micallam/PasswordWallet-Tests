using Dapper;
using Microsoft.Extensions.Configuration;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet
{
    public class DbContext : IDbContext
    {
        public string connectionString;

        private readonly IConfiguration _configuration;
        private readonly IDbConnection db;

        public DbContext(IConfiguration configuration)
        {
            if (configuration is IConfiguration)
            {
                _configuration = configuration;

                connectionString = _configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;

                db = new SqlConnection(connectionString);
            }
        }

        public int CreateUser(UserModel user)
        {
            string sqlQuery = "Insert Into Users (Salt, PasswordHash, Login, IsPasswordKeptAsHash) Values(@Salt, @PasswordHash, @Login, @IsPasswordKeptAsHash)";

            return db.Execute(sqlQuery, user);
        }

        public int DeleteUser(int userId)
        {
            string sqlQuery = $"delete from Users where Id = {userId}";

            return db.Execute(sqlQuery, userId);
        }

        public List<UserModel> GetUsersList()
        {
            return db.Query<UserModel>("Select * From Users").ToList();
        }

        public int UpdateUser(UserModel userToUpdate)
        {
            string sqlQuery = $"UPDATE Users " +
                $"set Salt='{userToUpdate.Salt}'" +
                $",PasswordHash='{userToUpdate.PasswordHash}'" +
                $",IsPasswordKeptAsHash='{userToUpdate.IsPasswordKeptAsHash}' " +
                $"WHERE Id={userToUpdate.Id}";

            return db.Execute(sqlQuery);
        }

        public UserModel GetUserById(int idUser)
        {
            return db.Query<UserModel>($"select * from Users where Id ={idUser}").SingleOrDefault();
        }

        public UserModel GetUserByLogin(string login)
        {
            return db.Query<UserModel>($"select * from Users where Login ='{login}'").SingleOrDefault();
        }

        public List<PasswordModel> GetPasswordListByUserId(int userId)
        {
            return db.Query<PasswordModel>($"Select * From Passwords where IdUser = {userId}").ToList(); ;
        }

        public int UpdatePasswordHash(int userId, string oldPassword, string newPassword)
        {
            return db.Execute(
                $"update Passwords " +
                $"set PasswordHash = '{newPassword}' " +
                $"where IdUser = {userId} " +
                $"and PasswordHash = '{oldPassword}'");
        }

        public int CreatePassword(PasswordModel password)
        {
            string sqlQuery = "Insert Into Passwords " +
                "(IdUser, Login, Description, WebAddress, PasswordHash) " +
                "Values(@IdUser, @Login, @Description, @WebAddress, @PasswordHash)";

            return db.Execute(sqlQuery, password);
        }

        public PasswordModel GetPasswordByHash(string passwordHash)
        {
            string sqlQuery = $"Select * From Passwords WHERE PasswordHash = '{passwordHash}'";

            return db.Query<PasswordModel>(sqlQuery).SingleOrDefault();
        }

        public int DeletePassword(string passwordHash)
        {
            string sqlQuery = $"Delete From Passwords WHERE PasswordHash = '{passwordHash}'";

            return db.Execute(sqlQuery);
        }

        public int LogUserLogin(int userId, LoginStatus loginStatus, string ipAddress)
        {
            LoginLog log = new LoginLog()
            {
                UserId = userId,
                LoginDateTime = DateTime.Now,
                LoginStatus = loginStatus,
                IpAddress = ipAddress
            };

            string sqlQuery = "Insert Into LoginLog " +
                "(UserId, LoginDateTime, LoginStatus, IpAddress) " +
                "Values(@UserId, @LoginDateTime, @LoginStatus, @IpAddress)";

            return db.Execute(sqlQuery, log);
        }

        public List<LoginLog> GetLoginLogListByUserId(int userId)
        {
            return db.Query<LoginLog>($"Select * From LoginLog where UserId = {userId}").ToList(); ;
        }

        public LoginBlocade GetBlocadeByUserId(int userId)
        {
            return db.Query<LoginBlocade>(sql: $"select * from LoginBlocade where UserId ='{userId}'")
                .SingleOrDefault();
        }

        public LoginBlocade GetBlocadeByIp(string ipAddress)
        {
            return db.Query<LoginBlocade>(sql: $"select * from LoginBlocade where IpAddress ='{ipAddress}'")
                .SingleOrDefault();
        }

        public int SaveUserBlocade(LoginBlocade loginBlocade)
        {
            string sqlQuery = "Insert Into LoginBlocade " +
                "(UserId, FailCount, BlockUntil) " +
                "Values(@UserId, @FailCount, @BlockUntil)";

            return db.Execute(sqlQuery, loginBlocade);
        }

        public int UpdateUserBlocade(LoginBlocade loginBlocade)
        {
            string sqlQuery = "Update LoginBlocade " +
                $"set FailCount = {loginBlocade.FailCount}, " +
                $"BlockUntil =  '{loginBlocade.BlockUntil:yyyy-MM-dd HH:mm:ss.fff}' " +
                $"where UserId = '{loginBlocade.UserId}'";

            return db.Execute(sqlQuery);
        }

        public int SaveIpBlocade(LoginBlocade loginBlocade)
        {
            string sqlQuery = "Insert Into LoginBlocade " +
                "(IpAddress, FailCount, BlockUntil) " +
                "Values(@IpAddress, @FailCount, @BlockUntil)";

            return db.Execute(sqlQuery, loginBlocade);
        }
        
        public int UpdateIpBlocade(LoginBlocade loginBlocade)
        {
            string sqlQuery = "Update LoginBlocade " +
                $"set FailCount = {loginBlocade.FailCount}, " +
                $"BlockUntil =  '{loginBlocade.BlockUntil:yyyy-MM-dd HH:mm:ss.fff}' " +
                $"where IpAddress = '{loginBlocade.IpAddress}'";

            return db.Execute(sqlQuery);
        }

        public int DeleteUserBlocade(int userId)
        {
            string sqlQuery = $"Delete From LoginBlocade " +
                $"WHERE UserId = {userId}";

            return db.Execute(sqlQuery);
        }

        public int DeleteIpBlocade(string ipAddress)
        {
            string sqlQuery = $"Delete From LoginBlocade " +
                $"WHERE IpAddress = '{ipAddress}'";

            return db.Execute(sqlQuery);
        }

        public LoginBlocade GetActiveBlocadeByUserId(int userId)
        {
            return db.Query<LoginBlocade>(
                sql: $"select * from LoginBlocade " +
                $"where UserId ='{userId}'" +
                $"and BlockUntil >= '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}'")
                    .SingleOrDefault();
        }

        public LoginBlocade GetActiveBlocadeByIp(string ipAddress)
        {
            return db.Query<LoginBlocade>(
                sql: $"select * from LoginBlocade " +
                $"where IpAddress ='{ipAddress}'" +
                $"and BlockUntil >= '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}'")
                    .SingleOrDefault();
        }
    }
}
