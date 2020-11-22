using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class PasswordModel
    {
        public int IdUser { get; set; }
        public string WebAddress { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Description { get; set; }

    }
}
