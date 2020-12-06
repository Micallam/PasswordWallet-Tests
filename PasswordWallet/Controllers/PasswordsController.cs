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
    public enum ViewMode
    {
        View,
        Edit
    }

    public class PasswordsController : Controller
    {
        private readonly IDbContext dbContext;
        protected UserInfo userInfo;

        private readonly IConfiguration _configuration;

        public PasswordsController(IConfiguration configuration = null, IDbContext dbContext = null)
        {
            if (configuration != null)
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

        protected ViewMode GetViewMode()
        {
            ViewMode mode;

            try
            {
                mode = (ViewMode)Enum.Parse(typeof(ViewMode), HttpContext.Session.GetString("ViewMode"));
            }
            catch
            {
                mode = ViewMode.View;
            }

            return mode;
        }

        protected void SetViewMode(ViewMode mode)
        {
            HttpContext.Session.SetString("ViewMode", mode.ToString());
        }

        [HttpPost]
        public ActionResult ChangeMode()
        {
            var viewMode = GetViewMode();
            
            SetViewMode(
                viewMode == ViewMode.View ? 
                ViewMode.Edit : 
                ViewMode.View);

            return RedirectToAction(nameof(Index));
        }

        // GET: Passwords
        public ActionResult Index()
        {
            userInfo = GetUserInfo();

            ViewBag.Message = GetViewMode();

            try
            {
                string message = HttpContext.Session.GetString("WarningMessage");

                if (message != "")
                {
                    ModelState.AddModelError("Warning", message);
                }
            }
            catch { }

            return View(dbContext.GetPasswordListByUserId(userInfo.Id));
        }

        // GET: Passwords/ShowLog
        public ActionResult ShowLog()
        {
            userInfo = GetUserInfo();

            return View(dbContext.GetLoginLogListByUserId(userInfo.Id));
        }

        protected UserInfo GetUserInfo()
        {
            UserInfo info;

            try
            {
                info = JsonConvert.DeserializeObject<UserInfo>(HttpContext.Session.GetString("UserInfo"));
            }
            catch
            {
                info = new UserInfo();
            }
            return info;
        }

        // GET: Passwords/SharePassword
        public ActionResult SharePassword(string passwordHash)
        {
            if (IsSharedPassword(
                passwordHash, 
                "You cannot share this password! You are not an owner."))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var password = dbContext.GetPasswordByHash(passwordHash);

                HttpContext.Session.SetString(
                    "PasswordShareInfo",
                    JsonConvert.SerializeObject(
                        new PasswordShareInfo()
                        {
                            OwnerId = GetUserInfo().Id,
                            PasswordId = password.Id,
                        }));

                return RedirectToAction(nameof(Index), "PasswordShare");
            }
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
                userInfo = GetUserInfo();

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
            userInfo = GetUserInfo();

            PasswordModel password = dbContext.GetPasswordByHash(passwordHash);
            if (password != null)
            {
                password.PasswordHash = EncryptionHelper.DecryptPasswordAES(password.PasswordHash, userInfo.LoggedUserPassword);

                return View(password);
            }
            else
            {
                return View(
                    "EnterSharingKey",
                    new SharePasswordKey()
                    {
                        KeyHash = passwordHash
                    });
            }
        }

        public bool IsSharedPassword(string passwordHash, string message = "")
        {
            if (dbContext.GetSharedPasswordByHash(passwordHash) == null)
            {
                HttpContext.Session.SetString(
                    "WarningMessage",
                    ""
                    );

                return false;
            }
            else
            {
                HttpContext.Session.SetString(
                    "WarningMessage",
                    message
                    );

                return true;
            }
        }

        [HttpPost]
        public ActionResult SharePasswordDatail(SharePasswordKey sharePasswordKey)
        {
            var sharedPassword = dbContext.GetSharedPasswordByHash(sharePasswordKey.KeyHash);

            try
            {
                sharedPassword.PasswordHash =
                    EncryptionHelper.DecryptPasswordAES(
                        sharedPassword.PasswordHash,
                        sharePasswordKey.Key
                        );

                if (ValidateDecryptedPassword(sharedPassword.PasswordHash))
                {
                    throw new Exception();
                }

                HttpContext.Session.SetString(
                    "WarningMessage",
                    ""
                    );

                return View("SharedPasswordDetails", sharedPassword);
            }
            catch 
            {
                HttpContext.Session.SetString(
                    "WarningMessage",
                    "Wrong sharing key!"
                    );

                return RedirectToAction(nameof(Index));
            }
        }

        public bool ValidateDecryptedPassword(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }

        // GET: Passwords/Delete/5
        public ActionResult Delete(string passwordHash)
        {
            if (ValidateViewMode("Cannot delete. You are in view mode!") &&
                !IsSharedPassword(
                    passwordHash,
                    "You cannot delete this password! You are not an owner."))
            {
                return View(dbContext.GetPasswordByHash(passwordHash));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Passwords/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, IFormCollection collection)
        {
            try
            {
                if (ValidateViewMode("Cannot delete. You are in view mode!"))
                {
                    string passwordHash = collection["PasswordHash"];

                    dbContext.DeletePassword(passwordHash);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        protected bool ValidateViewMode(string message = "")
        {
            bool isOk;

            if (GetViewMode() == ViewMode.Edit)
            {
                HttpContext.Session.SetString(
                    "WarningMessage",
                    ""
                    );

                isOk = true;
            }
            else
            {
                HttpContext.Session.SetString(
                    "WarningMessage",
                    message
                    );

                isOk = false;
            }

            return isOk;
        }

    }
}