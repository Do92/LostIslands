namespace Miscellaneous
{
    public class MD5
    {
        // Encrypt a string to a MD5 hash
        public static string Encrypt(string valueToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(valueToEncrypt);

            // Encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');

            return hashString.PadLeft(32, '0');
        }
    }
}