using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class UserPasswords
    {
        public int Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string PasswordHash { get; set; }
        public bool IsPasswordKeptAsHash { get; set; }
    }
}
