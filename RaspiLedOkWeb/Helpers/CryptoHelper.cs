using System.Security.Cryptography;
using System.Text;

namespace RaspiLedOkWeb.Helpers
{
    public static class CryptoHelper
    {
        public static string PreEncrypt(string toEncrypt, bool useHashing = false)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

            // If hashing use get hashcode regards to your key
            if (useHashing)
            {
                using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                {
                    keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes("inCH3i0T@123"));
                    // MD5 produces a 16-byte hash which is valid for TripleDES
                }
            }
            else
            {
                // If not using hashing, we need to ensure the key is the correct length
                string key = "inCH3i0T@123";
                // Pad or truncate the key to exactly 24 bytes (preferred) or 16 bytes
                if (key.Length < 24)
                {
                    // Pad the key to 24 characters (bytes)
                    key = key.PadRight(24, '#');
                }
                else if (key.Length > 24)
                {
                    // Truncate to 24 characters
                    key = key.Substring(0, 24);
                }
                keyArray = Encoding.UTF8.GetBytes(key);
            }

            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform cTransform = tdes.CreateEncryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return Convert.ToBase64String(resultArray);
                }
            }
        }

        public static string PreDecrypt(string cipherString, bool useHashing = false)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            if (useHashing)
            {
                using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                {
                    keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes("inCH3i0T@123"));
                }
            }
            else
            {
                string key = "inCH3i0T@123";
                // Ensure the key is the same length as used for encryption
                if (key.Length < 24)
                {
                    key = key.PadRight(24, '#');
                }
                else if (key.Length > 24)
                {
                    key = key.Substring(0, 24);
                }
                keyArray = Encoding.UTF8.GetBytes(key);
            }

            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform cTransform = tdes.CreateDecryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return Encoding.UTF8.GetString(resultArray);
                }
            }
        }
    }
}