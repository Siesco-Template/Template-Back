using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Utilies.PasswordUtilities
{
    public static class PasswordChecker
    {

        private static readonly string LowerChars = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string Digits = "0123456789";
        private static readonly string Symbols = "!@#$%&*";

        private static readonly Random random = new();

        public static string GenerateRandomPassword()
        {
            int length = random.Next(8, 13); // 8 ile 12 arasında rastgele uzunluk

            // Gerekli karakter tiplerinden en az birer tane ekle
            var passwordChars = new StringBuilder();
            passwordChars.Append(UpperChars[random.Next(UpperChars.Length)]);
            passwordChars.Append(Digits[random.Next(Digits.Length)]);
            passwordChars.Append(Symbols[random.Next(Symbols.Length)]);

            // Kalan karakterleri rastgele ekle
            string allChars = LowerChars + UpperChars + Digits + Symbols;
            for (int i = passwordChars.Length; i < length; i++)
            {
                passwordChars.Append(allChars[random.Next(allChars.Length)]);
            }

            // Karakterleri karıştır (Shuffle)
            return new string(passwordChars.ToString().ToCharArray()
                .OrderBy(_ => random.Next())
                .ToArray());
        }

        public static bool CheckPassword(string password)
        {
            if (password.Length >= 8 &&
                password.Length <= 64 &&
                password.Any(char.IsUpper) &&
                password.Any(char.IsNumber) &&
                password.Any(char.IsPunctuation))
                return true;

            return false;
        }

        public static void CheckPasswordAndThrowException(string password)
        {
            if (!(password.Length >= 8 &&
                password.Length <= 64 &&
                password.Any(char.IsUpper) &&
                password.Any(char.IsNumber) &&
                password.Any(char.IsPunctuation)))
            throw new Exception("Ən az 8 xarakter, 1 böyük hərf, 1 rəqəm və simvol");
        }
    }
}
