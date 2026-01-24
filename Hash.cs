using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Belpost.Auth
{
    class Hash 
    {
        public static class PasswordHasher
        {
            public static (string hash, string salt) Hash(string password)
            {

                var saltBytes = RandomNumberGenerator.GetBytes(16);


                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
                var hashBytes = pbkdf2.GetBytes(32); 

                return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
            }


            public static bool Verify(string password, string hash, string salt)
            {
                var saltBytes = Convert.FromBase64String(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(32);

                return Convert.ToBase64String(computedHash) == hash;
            }
        }
    }

}
