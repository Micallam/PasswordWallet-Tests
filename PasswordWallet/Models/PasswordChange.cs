using System;

namespace PasswordWallet.Models
{
    public class PasswordChange
    {
        public int Id { get; set; }
        public int PasswordId { get; set; }
        public int UserId { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public DateTime ChangeDateTime { get; set; }

        public static string PasswordToString(PasswordModel password)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(password);
        }

        public static PasswordModel StringToPassword(string serializedObject)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PasswordModel>(serializedObject);
        }

        public static void LogPasswordCreate(PasswordModel password, IDbContext dbContext)
        {
            PasswordChange passwordChange = new PasswordChange()
            {
                PasswordId = password.Id,
                UserId = password.IdUser,
                NewData = PasswordToString(password),
                ChangeDateTime = LoginHelper.TruncateDateTime(DateTime.Now)
            };

            dbContext.LogPasswordChange(passwordChange);
        }

        public static void LogPasswordEdit(PasswordModel old, PasswordModel changed, IDbContext dbContext)
        {
            PasswordChange passwordChange = new PasswordChange()
            {
                PasswordId = changed.Id,
                UserId = changed.IdUser,
                OldData = PasswordToString(old),
                NewData = PasswordToString(changed),
                ChangeDateTime = LoginHelper.TruncateDateTime(DateTime.Now)
            };

            dbContext.LogPasswordChange(passwordChange);
        }

        public static void LogPasswordDelete(PasswordModel password, IDbContext dbContext)
        {
            PasswordChange passwordChange = new PasswordChange()
            {
                PasswordId = password.Id,
                UserId = password.IdUser,
                OldData = PasswordToString(password),
                ChangeDateTime = LoginHelper.TruncateDateTime(DateTime.Now)
            };

            dbContext.LogPasswordChange(passwordChange);
        }
    }
}
