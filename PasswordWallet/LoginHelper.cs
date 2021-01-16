using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet
{
    public enum TimeInterval
    {
        Seconds,
        Minutes
    }

    public class LoginHelper
    {
        readonly IDbContext dbContext;

        public LoginHelper(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public LoginBlocade CreateOrUpdateLoginUserIdBlocade(int userId, TimeInterval interval = TimeInterval.Seconds)
        {
            LoginBlocade loginBlocade = dbContext.GetBlocadeByUserId(userId);

            if (loginBlocade == null)
            {
                loginBlocade = new LoginBlocade()
                {
                    UserId = userId,
                    FailCount = 1,
                    BlockUntil = TruncateDateTime(DateTime.Now)
                };

                dbContext.SaveUserBlocade(loginBlocade);
            }
            else
            {
                loginBlocade = UpdateBlocadeParams(loginBlocade, interval);

                dbContext.UpdateUserBlocade(loginBlocade);
            }

            return loginBlocade;
        }

        public LoginBlocade CreateOrUpdateLoginIpBlocade(string ipAddress, TimeInterval interval = TimeInterval.Seconds)
        {
            LoginBlocade loginBlocade = dbContext.GetBlocadeByIp(ipAddress);

            if (loginBlocade == null)
            {
                loginBlocade = new LoginBlocade()
                {
                    IpAddress = ipAddress,
                    FailCount = 1,
                    BlockUntil = TruncateDateTime(DateTime.Now)
                };

                dbContext.SaveIpBlocade(loginBlocade);
            }
            else
            {
                loginBlocade = UpdateBlocadeParams(loginBlocade, interval);

                dbContext.UpdateIpBlocade(loginBlocade);
            }

            return loginBlocade;
        }

        protected LoginBlocade UpdateBlocadeParams(LoginBlocade loginBlocade, TimeInterval interval = TimeInterval.Seconds)
        {
            loginBlocade.FailCount += 1;

            if (loginBlocade.FailCount == 2)
            {
                loginBlocade.BlockUntil = TruncateDateTime(DateTime.Now).AddSeconds(5);
            }
            else if (loginBlocade.FailCount == 3)
            {
                loginBlocade.BlockUntil = TruncateDateTime(DateTime.Now).AddSeconds(10);
            }
            else if (loginBlocade.FailCount > 3)
            {
                loginBlocade.BlockUntil = TruncateDateTime(DateTime.Now).AddSeconds(60);
            }

            return loginBlocade;
        }

        public int ClearUserBlocade(int userId)
        {
            return dbContext.DeleteUserBlocade(userId);
        }

        public int ClearIpBlocade(string ipAddress)
        {
            return dbContext.DeleteIpBlocade(ipAddress);
        }

        public static DateTime TruncateDateTime(DateTime dateTime, TimeInterval interval = TimeInterval.Seconds)
        {
            switch (interval)
            {
                case TimeInterval.Seconds:
                    dateTime = dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));

                    break;

                case TimeInterval.Minutes:
                    dateTime = dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerMinute));

                    break;
            }

            return dateTime;
        }
    }

    public class LoginBlocadeException : Exception
    {
        public LoginBlocadeException(string message) : base(message)
        {
        }
    }

    public class UnknownLoginException : Exception
    {
        public UnknownLoginException(string message = "Unknown login") : base(message)
        {
        }
    }
}
