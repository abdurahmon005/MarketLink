using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service
{
    public  class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 65536,
                Iterations = 4
            };

            var hash = argon2.GetBytes(32);
 
            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }
    }
}
