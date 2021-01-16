using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public ActionType ActionType { get; set; }
        public int UserId { get; set; }
        public DateTime ActionDateTime { get; set; }

        public static void Log(int userId, ActionType actionType, IDbContext dbContext)
        {
            ActivityLog activity = new ActivityLog
            {
                ActionType = actionType,
                UserId = userId,
                ActionDateTime = LoginHelper.TruncateDateTime(DateTime.Now)
            };

            dbContext.LogActivity(activity);
        }
    }

    public enum ActionType
    {
        CreatePassword = 0,
        DeletePassword = 1,
        UpdatePassword = 2,
        ViewPassword = 3,
        SharePassword = 4
    }
}
