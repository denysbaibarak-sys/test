using System;
using System.Security.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 10000;

    public static string HashPassword(string password)
    {
        using (var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations))
        {
            var salt = algorithm.Salt;
            var key = algorithm.GetBytes(KeySize);

            var hashParts = new byte[SaltSize + KeySize];

            Buffer.BlockCopy(salt, 0, hashParts, 0, SaltSize);
            Buffer.BlockCopy(key, 0, hashParts, SaltSize, KeySize);

            return Convert.ToBase64String(hashParts);
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var hashParts = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(hashParts, 0, salt, 0, SaltSize);

        var key = new byte[KeySize];
        Buffer.BlockCopy(hashParts, SaltSize, key, 0, KeySize);

        using (var algorithm = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations))
        {
            var keyToCheck = algorithm.GetBytes(KeySize);

            return SlowEquals(key, keyToCheck);
        }
    }

    private static bool SlowEquals(byte[] a, byte[] b)
    {
        uint diff = (uint)a.Length ^ (uint)b.Length;

        for (int i = 0; i < a.Length && i < b.Length; i++)
        {
            diff |= (uint)(a[i] ^ b[i]);
        }

        return diff == 0;
    }
}