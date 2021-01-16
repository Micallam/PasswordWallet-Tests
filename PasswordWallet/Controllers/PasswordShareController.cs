using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PasswordWallet.Models;

namespace PasswordWallet.Controllers
{
    public class PasswordShareController : Controller
    {
        private readonly IDbContext dbContext;
        private readonly IConfiguration _configuration;

        public PasswordShareController(IConfiguration configuration = null, IDbContext dbContext = null)
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

        // GET: PasswordShareController
        public ActionResult Index()
        {
            var shareInfo = GetPasswordShareInfo();
            var userInfo = GetUserInfo();

            var sharePasswordKeys = dbContext.GetSharePasswordKeyListByOwnerIdAndPasswordId(
                shareInfo.OwnerId,
                shareInfo.PasswordId);

            foreach (var key in sharePasswordKeys)
            {
                key.KeyHash = EncryptionHelper.DecryptPasswordAES(
                    key.KeyHash,
                    userInfo.LoggedUserPassword);
            }

            return View(sharePasswordKeys);
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

        protected PasswordShareInfo GetPasswordShareInfo()
        {
            PasswordShareInfo info;

            try
            {
                info = JsonConvert.DeserializeObject<PasswordShareInfo>(HttpContext.Session.GetString("PasswordShareInfo"));
            }
            catch
            {
                info = new PasswordShareInfo();
            }
            return info;
        }

        // GET: PasswordShareController/Create
        public ActionResult Create()
        {
             return View();
        }

        // POST: PasswordShareController/Create
        [HttpPost]
        public ActionResult Create(SharePasswordKey sharePasswordKey)
        {
            try
            {
                var shareInfo = GetPasswordShareInfo();
                
                SharePassword(sharePasswordKey);

                sharePasswordKey.OwnerId = shareInfo.OwnerId;
                sharePasswordKey.PasswordId = shareInfo.PasswordId;
                sharePasswordKey.KeyHash = EncryptionHelper.EncryptPasswordAES(
                    sharePasswordKey.KeyHash,
                    GetUserInfo().LoggedUserPassword);

                dbContext.CreateSharePasswordKey(sharePasswordKey);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public void SharePassword(SharePasswordKey sharePasswordKey)
        {
            var shareInfo = GetPasswordShareInfo();
            var userInfo = GetUserInfo();
            var passToShare = dbContext.GetPassword(shareInfo.PasswordId);

            dbContext.CreateSharedPassword(
                new SharedPassword() 
                {
                    Login = passToShare.Login,
                    PasswordHash = ChangePasswordHash(
                        passToShare.PasswordHash, 
                        userInfo.LoggedUserPassword, 
                        sharePasswordKey.KeyHash),
                    SharedForUser = sharePasswordKey.SharedForUser,
                    OwnerId = shareInfo.OwnerId,
                    WebAddress = passToShare.WebAddress,
                    Description = passToShare.Description
                });

            ActivityLog.Log(
                userInfo.Id,
                ActionType.SharePassword,
                dbContext);
        }

        private string ChangePasswordHash(string oldHash, string oldKey, string newKey)
        {
            return EncryptionHelper.EncryptPasswordAES(
                EncryptionHelper.DecryptPasswordAES(
                    oldHash, 
                    oldKey), 
                newKey);
        }

        // GET: PasswordShareController/Delete/5
        public ActionResult Delete(int id)
        {
            return View(dbContext.GetSharePasswordKey(id));
        }

        // POST: PasswordShareController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                DeleteRelatedSharedPassword(dbContext.GetSharePasswordKey(id));
                dbContext.DeleteSharePasswordKey(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private void DeleteRelatedSharedPassword(SharePasswordKey sharePasswordKey)
        {
            var userInfo = GetUserInfo();
            var password = dbContext.GetPassword(sharePasswordKey.PasswordId);

            var decryptedPass =
                EncryptionHelper.DecryptPasswordAES(
                    password.PasswordHash,
                    userInfo.LoggedUserPassword);

            var sharedPasswordHash = EncryptionHelper.EncryptPasswordAES(
                decryptedPass,
                EncryptionHelper.DecryptPasswordAES(
                    sharePasswordKey.KeyHash,
                    userInfo.LoggedUserPassword));
            
            dbContext.DeleteSharedPasswordByHash(sharedPasswordHash);
        }
    }
}
