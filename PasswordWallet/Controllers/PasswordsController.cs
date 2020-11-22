using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PasswordWallet.Models;
using PasswordWallet;
using Newtonsoft.Json;

namespace PasswordWallet.Controllers
{
    public class PasswordsController : Controller
    {
        private readonly IDbContext dbContext;
        protected UserInfo userInfo;

        private readonly IConfiguration _configuration;

        public PasswordsController(IConfiguration configuration = null, IDbContext dbContext = null)
        {
            if (_configuration is IConfiguration)
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

        // GET: Passwords
        public ActionResult Index()
        {
            userInfo = JsonConvert.DeserializeObject<UserInfo>(HttpContext.Session.GetString("UserInfo"));

            return View(dbContext.GetPasswordListByUserId(userInfo.Id));
        }

        // GET: Passwords/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Passwords/Create
        [HttpPost]
        public ActionResult Create(PasswordModel password)
        {
            try
            {
                userInfo = JsonConvert.DeserializeObject<UserInfo>(HttpContext.Session.GetString("UserInfo"));

                UserModel user;

                user = dbContext.GetUserById(userInfo.Id);

                password.IdUser = userInfo.Id;
                password.PasswordHash = EncryptionHelper.EncryptPasswordAES(password.PasswordHash, userInfo.LoggedUserPassword);

                CreatePassword(password);

                return RedirectToAction(nameof(Index), new { idUser = password.IdUser });
            }
            catch
            {
                return View();
            }
        }

        public int CreatePassword(PasswordModel password)
        {
            if (password.IdUser == 0 || password.PasswordHash == "")
            {
                throw new ArgumentNullException();
            }

            return dbContext.CreatePassword(password);
        }

        // GET: Passwords/Details/5
        public ActionResult Details(string passwordHash)
        {
            userInfo = JsonConvert.DeserializeObject<UserInfo>(HttpContext.Session.GetString("UserInfo"));

            PasswordModel password = dbContext.GetPasswordByHash(passwordHash);
            
            password.PasswordHash = EncryptionHelper.DecryptPasswordAES(password.PasswordHash, userInfo.LoggedUserPassword);

            return View(password);
        }

        // GET: Passwords/Delete/5
        public ActionResult Delete(string passwordHash)
        {
            return View(dbContext.GetPasswordByHash(passwordHash));
        }

        // POST: Passwords/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, IFormCollection collection)
        {
            try
            {
                string passwordHash = collection["PasswordHash"];

                dbContext.DeletePassword(passwordHash);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }
}