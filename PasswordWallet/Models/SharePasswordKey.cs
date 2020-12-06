using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Models
{
    public class SharePasswordKey
    {
        public int Id { get; set; }
        public int PasswordId { get; set; }
        public int SharedForUser { get; set; }
        public int OwnerId { get; set; }
        public string Key { get; set; }
        public string KeyHash { get; set; }
    }
}
