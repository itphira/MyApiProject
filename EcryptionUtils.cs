using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionUtils
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("MiContrasenyade32charsQWEASDZXCR");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("MiContrasenya16D");

    public static string Encrypt(string plainText)
    {
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public static string Decrypt(string cipherText)
    {
        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(fullCipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    // Leer los datos descifrados
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            // Manejar errores de descifrado (por ejemplo, clave incorrecta, texto cifrado inválido)
            return $"Error durante el descifrado: {ex.Message}";
        }
    }
}
