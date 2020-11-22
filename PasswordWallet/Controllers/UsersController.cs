using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PasswordWallet;
using System.Configuration;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace PasswordWallet.Controllers
{
    public class UsersController : Controller
    {
        private static readonly Random random = new Random();
        private const string  pepper = "Pepper";

        private readonly IDbContext dbContext;
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration = null, IDbContext dbContext = null)
        {
            if (configuration is IConfiguration)
            {
                _configuration = configuration;
            }

            if (dbContext != null)
            {
                this.dbContext = dbContext;
            }
            else if (_configuration != null)
            {
                this.dbContext = new DbContext(configuration);
            }
        }

        // GET: Users
        public ActionResult Index()
        {
            return View(GetUserList());
        }

        public List<UserModel> GetUserList()
        {
            return dbContext.GetUsersList();
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserModel user)
        {
            try
            {
                user.Salt = GetSalt();
                user.PasswordHash = user.IsPasswordKeptAsHash ?
                    GetPasswordHashSHA512(user.PasswordHash, user.Salt, pepper) :
                    GetPasswordHMAC(user.PasswordHash, user.Salt, pepper);

                CreateUser(user);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public int CreateUser(UserModel user)
        {
            if (dbContext.GetUserByLogin(user.Login) != null)
            {
                throw new DuplicateNameException(user.Login);
            }

            return dbContext.CreateUser(user);
        }

        // GET: Users/Edit/5
        public ActionResult Login()
        {
            UserLogin userLogin = new UserLogin();

            return View(userLogin);
        }

        [HttpPost]
        public ActionResult Login(UserLogin userLogin)
        {
            int loggedUserId = CheckLoginCridentials(userLogin);

            if (loggedUserId != 0)
            {
                HttpContext.Session.SetString(
                    "UserInfo", 
                    JsonConvert.SerializeObject(
                        new UserInfo() 
                        { 
                            Id = loggedUserId, 
                            LoggedUserPassword = userLogin.Password 
                        }));

                return RedirectToAction(nameof(Index), "Passwords");
            }

            return RedirectToAction(nameof(Index));
        }

        public int CheckLoginCridentials(UserLogin userLogin)
        {
            UserModel user = dbContext.GetUserByLogin(userLogin.Login);

            string userPassHash = user.PasswordHash;
            string loginPassHash = user.IsPasswordKeptAsHash ?
                GetPasswordHashSHA512(userLogin.Password, user.Salt, pepper) :
                GetPasswordHMAC(userLogin.Password, user.Salt, pepper);

            return userPassHash == loginPassHash ? user.Id : 0;
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int id)
        {
            UserModel user = dbContext.GetUserById(id);
            UserPasswords userPasswords = new UserPasswords
            {
                Id = user.Id,
                PasswordHash = user.PasswordHash,
                IsPasswordKeptAsHash = user.IsPasswordKeptAsHash
            };

            return View(userPasswords);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UserPasswords userPasswords)
        {
            try
            {
                UserModel users = dbContext.GetUserById(userPasswords.Id);

                UserModel editedUser = new UserModel
                {
                    Salt = GetSalt(),
                    Id = userPasswords.Id,
                    IsPasswordKeptAsHash = userPasswords.IsPasswordKeptAsHash
                };

                string oldPassHash = users.IsPasswordKeptAsHash ?
                        GetPasswordHashSHA512(userPasswords.OldPassword, users.Salt, pepper) :
                        GetPasswordHMAC(userPasswords.OldPassword, users.Salt, pepper);

                if (users.PasswordHash == oldPassHash)
                {
                    editedUser.PasswordHash = userPasswords.IsPasswordKeptAsHash ?
                        GetPasswordHashSHA512(userPasswords.NewPassword, editedUser.Salt, pepper) :
                        GetPasswordHMAC(userPasswords.NewPassword, editedUser.Salt, pepper);

                    this.UpdateUserPasswords(editedUser, userPasswords.OldPassword, userPasswords.NewPassword);

                    dbContext.UpdateUser(editedUser);

                    return RedirectToAction(nameof(Index));
                }

                return View();
            }
            catch
            {
                return View();
            }
        }

        public int UpdateUserPasswords(UserModel user, string oldPassword, string newPassword)
        {
            List<PasswordModel> passwords;
            int affectedRows = 0;

            passwords = dbContext.GetPasswordListByUserId(user.Id);

            for (int i = passwords.Count() - 1; i >= 0; i--)
            {
                String encryptedPassword = passwords[i].PasswordHash;
                String decryptedPassword = EncryptionHelper.DecryptPasswordAES(encryptedPassword, oldPassword);

                affectedRows += dbContext.UpdatePasswordHash(
                    user.Id, 
                    encryptedPassword,
                    EncryptionHelper.EncryptPasswordAES(decryptedPassword, newPassword));
            }

            return affectedRows;
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int id)
        {
            return View(dbContext.GetUserById(id));
        }

        // POST: Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                dbContext.DeleteUser(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public string GetSalt()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetPasswordHashSHA512(string _password, string _salt, string _pepper = pepper)
        {
            return CalculateSHA512(_password + _salt + _pepper);
        }

        public static string CalculateSHA512(string _secretText)
        {
            string secretText = _secretText ?? "";
            var encoding = new ASCIIEncoding();

            byte[] secretTextBytes = encoding.GetBytes(secretText);

            using (var sha512 = SHA512.Create())
            {
                byte[] hashmessage = sha512.ComputeHash(secretTextBytes);

                return Convert.ToBase64String(hashmessage);
            }
        }
        
        public static string GetPasswordHMAC(string _password, string _salt, string _pepper = pepper)
        {
            return CalculateHMAC(_password, _salt + _pepper);
        }

        public static string CalculateHMAC(string _secretText, string _key)
        {
            string secretText = _secretText ?? "";
            var encoding = new ASCIIEncoding();

            byte[] secretTextBytes = encoding.GetBytes(secretText);
            byte[] keyBytes = encoding.GetBytes(_key);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashmessage = hmac.ComputeHash(secretTextBytes);

                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}