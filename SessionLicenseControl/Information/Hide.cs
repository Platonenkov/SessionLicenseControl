using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SessionLicenseControl.Information
{
    internal static class Hide
    {
        /// <summary>
        /// Массив байт - "соль" алгоритма шифрования Rfc2898
        /// </summary>
        private static readonly byte[] SALT =
        {
            0x26, 0xdc, 0xff, 0x00,
            0xad, 0xed, 0x7a, 0xee,
            0xc5, 0xfe, 0x07, 0xaf,
            0x4d, 0x08, 0x22, 0x3c
        };

        /// <summary>Получить алгоритм шифрования с указанным паролем</summary>
        /// <param name="password">Пароль шифрования</param>
        /// <returns>Алгоритм шифрования</returns>
        private static ICryptoTransform GetAlgorithm(string password)
        {
            var pdb = new Rfc2898DeriveBytes(password, SALT);
            var algorithm = Rijndael.Create();
            algorithm.Key = pdb.GetBytes(32);
            algorithm.IV = pdb.GetBytes(16);
            return algorithm.CreateEncryptor();
        }

        /// <summary>Получить алгоритм для расшифровки</summary>
        /// <param name="password">Пароль</param>
        /// <returns>Алгоритм расшифровки</returns>
        private static ICryptoTransform GetInverseAlgorithm(string password)
        {
            var pdb = new Rfc2898DeriveBytes(password, SALT);
            var algorithm = Rijndael.Create();
            algorithm.Key = pdb.GetBytes(32);
            algorithm.IV = pdb.GetBytes(16);
            return algorithm.CreateDecryptor();
        }

        /// <summary>Зашифровать массив байт</summary>
        /// <param name="data">Шифруемая последовательность байт</param>
        /// <param name="password">Ключ шифрования</param>
        /// <returns>Зашифрованная последовательность байт</returns>
        public static byte[] Cover(this byte[] data, string password)
        {
            var algorithm = GetAlgorithm(password);
            using var stream = new MemoryStream();
            using var crypto_stream = new CryptoStream(stream, algorithm, CryptoStreamMode.Write);
            crypto_stream.Write(data, 0, data.Length);
            crypto_stream.FlushFinalBlock();
            return stream.ToArray();
        }

        /// <summary>Зашифровать строку</summary>
        /// <param name="str">Шифруемая строка</param>
        /// <param name="password">Пароль шифрования</param>
        /// <returns>Зашифрованная строка</returns>
        public static string Cover(this string str, string password) => Convert.ToBase64String(str.Compress().Cover(password));

        /// <summary>Расшифровать последовательность байт</summary>
        /// <param name="data">Расшифровываемая последовательность байт</param>
        /// <param name="password">Пароль шифрования</param>
        /// <returns>Расшифрованная последовательность байт</returns>
        public static byte[] Discover(this byte[] data, string password)
        {
            var algorithm = GetInverseAlgorithm(password);
            using var stream = new MemoryStream();
            using var crypto_stream = new CryptoStream(stream, algorithm, CryptoStreamMode.Write);
            crypto_stream.Write(data, 0, data.Length);
            crypto_stream.FlushFinalBlock();
            return stream.ToArray();
        }

        /// <summary>Расшифровать строку</summary>
        /// <param name="str">Зашифрованная строка</param>
        /// <param name="password">Пароль шифрования</param>
        /// <returns>Расшифрованная строка</returns>
        public static string Discover(this string str, string password) => Convert.FromBase64String(str).Discover(password).DecompressAsString();
    }
}
