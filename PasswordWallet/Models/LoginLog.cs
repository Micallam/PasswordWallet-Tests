using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class LoginLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginDateTime { get; set; }
        public LoginStatus LoginStatus { get; set; }
        public string IpAddress { get; set; }

    }

    public enum LoginStatus
    {
        Failed = 0,
        Success = 1
    }
}
