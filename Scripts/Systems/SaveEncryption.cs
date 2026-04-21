using Godot;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Environment = Godot.Environment;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Handles save data encryption and decryption
    /// Currently a stub - can be expanded for future encryption needs
    /// </summary>
    public class SaveEncryption
    {
        // 改进的密钥生成: 使用机器指纹混合盐值，而非硬编码
        private static byte[] GetMachineKey()
        {
            // 使用机器名和用户名作为盐值
            string machineFingerprint = $"{System.Environment.MachineName}_{Environment.UserName}_ClawRPG";
            byte[] salt = Encoding.UTF8.GetBytes(machineFingerprint);
            
            // 使用 PBKDF2 派生密钥
            using (var deriveBytes = new Rfc2898DeriveBytes("ClawRPG_Salt_2024", salt, 10000, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(32); // 256-bit key
            }
        }
        
        private static byte[] GetMachineIV()
        {
            string machineFingerprint = $"{System.Environment.MachineName}_{Environment.UserName}_ClawRPG";
            byte[] salt = Encoding.UTF8.GetBytes(machineFingerprint);
            
            using (var deriveBytes = new Rfc2898DeriveBytes("ClawRPG_IV_Salt", salt, 10000, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(16); // 128-bit IV
            }
        }
        
        // 缓存派生后的密钥
        private static byte[] _cachedKey;
        private static byte[] _cachedIV;
        
        private static byte[] DefaultKey {
            get {
                if (_cachedKey == null)
                    _cachedKey = GetMachineKey();
                return _cachedKey;
            }
        }
        
        private static byte[] DefaultIV {
            get {
                if (_cachedIV == null)
                    _cachedIV = GetMachineIV();
                return _cachedIV;
            }
        }
        
        /// <summary>
        /// Whether encryption is enabled
        /// Currently disabled by default - enable when needed
        /// </summary>
        public bool IsEnabled { get; private set; } = false;
        
        /// <summary>
        /// Enable encryption with default key
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
            GD.Print("[SaveEncryption] Encryption enabled");
        }
        
        /// <summary>
        /// Disable encryption
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            GD.Print("[SaveEncryption] Encryption disabled");
        }
        
        /// <summary>
        /// Encrypt string data (AES-256)
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;
                
            if (!IsEnabled)
                return plainText;
                
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = DeriveKey(DefaultKey);
                    aes.IV = DeriveIV(DefaultIV);
                    
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveEncryption] Encryption failed: " + e.Message);
                return plainText;
            }
        }
        
        /// <summary>
        /// Decrypt string data (AES-256)
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;
                
            if (!IsEnabled)
                return cipherText;
                
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = DeriveKey(DefaultKey);
                    aes.IV = DeriveIV(DefaultIV);
                    
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    
                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveEncryption] Decryption failed: " + e.Message);
                return cipherText;
            }
        }
        
        /// <summary>
        /// Encrypt file content
        /// </summary>
        public bool EncryptFile(string sourcePath, string destPath)
        {
            if (!System.IO.File.Exists(sourcePath))
                return false;
                
            try
            {
                string content = System.IO.File.ReadAllText(sourcePath);
                string encrypted = Encrypt(content);
                System.IO.File.WriteAllText(destPath, encrypted);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveEncryption] File encryption failed: " + e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Decrypt file content
        /// </summary>
        public bool DecryptFile(string sourcePath, string destPath)
        {
            if (!System.IO.File.Exists(sourcePath))
                return false;
                
            try
            {
                string content = System.IO.File.ReadAllText(sourcePath);
                string decrypted = Decrypt(content);
                System.IO.File.WriteAllText(destPath, decrypted);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveEncryption] File decryption failed: " + e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Derive a 256-bit key from the provided seed
        /// </summary>
        private byte[] DeriveKey(byte[] seed)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(seed);
            }
        }
        
        /// <summary>
        /// Derive a 128-bit IV from the provided seed
        /// </summary>
        private byte[] DeriveIV(byte[] seed)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(seed);
            }
        }
        
        /// <summary>
        /// Generate a secure random key
        /// </summary>
        public static byte[] GenerateRandomKey(int length = 32)
        {
            byte[] key = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return key;
        }
        
        /// <summary>
        /// Generate a secure random IV
        /// </summary>
        public static byte[] GenerateRandomIV(int length = 16)
        {
            return GenerateRandomKey(length);
        }
        
        /// <summary>
        /// Hash data using SHA-256 (for integrity checks)
        /// </summary>
        public static string HashData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;
                
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        
        /// <summary>
        /// Verify data integrity using hash
        /// </summary>
        public static bool VerifyIntegrity(string data, string expectedHash)
        {
            string actualHash = HashData(data);
            return actualHash == expectedHash;
        }
        
        /// <summary>
        /// Create a MAC (Message Authentication Code) for data
        /// </summary>
        public static string CreateMac(string data, byte[] key)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;
                
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = hmac.ComputeHash(dataBytes);
                return Convert.ToBase64String(hash);
            }
        }
        
        /// <summary>
        /// Verify MAC for data
        /// </summary>
        public static bool VerifyMac(string data, string mac, byte[] key)
        {
            string expectedMac = CreateMac(data, key);
            return expectedMac == mac;
        }
    }
}
