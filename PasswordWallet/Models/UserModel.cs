using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
        public string Login { get; set; }
        public bool IsPasswordKeptAsHash { get; set; }
    }
}
