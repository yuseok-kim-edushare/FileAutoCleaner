using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileAutoCleaner 
{
    /// <summary>
    /// Utility class providing custom encryption and decryption functions.
    /// 사용자 정의 암호화 및 복호화 기능을 제공하는 유틸리티 클래스
    /// </summary>
    public class CustomEncryption
    {
        // You need a secure way to store the encryption key and IV (hardcoded here for example, but use a safer method in production)
        // 암호화 키와 IV를 안전하게 보관하는 방식 필요 (이 예제에서는 하드코딩하지만 실제론 더 안전한 방법 사용)
        // In real environments, manage keys externally or use Windows Data Protection API
        // 실제 환경에서는 키를 외부에서 관리하거나 Windows Data Protection API를 통해 보호된 방식으로 저장
        private static readonly byte[] EncryptionKey = new byte[32] { 
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };
        
        private static readonly byte[] EncryptionIV = new byte[16] { 
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };

        /// <summary>
        /// Encrypts a string.
        /// 문자열을 암호화합니다.
        /// </summary>
        /// <param name="plainText">String to encrypt. 암호화할 문자열</param>
        /// <returns>Encrypted Base64 string. 암호화된 Base64 문자열</returns>
        public static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            byte[] encrypted;
            
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = EncryptionKey;
                aesAlg.IV = EncryptionIV;
                
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts an encrypted string.
        /// 암호화된 문자열을 복호화합니다.
        /// </summary>
        /// <param name="encryptedText">Base64-encoded encrypted string. Base64로 인코딩된 암호화 문자열</param>
        /// <returns>Decrypted original string. 복호화된 원본 문자열</returns>
        public static string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            string plaintext = null;
            byte[] cipherText = Convert.FromBase64String(encryptedText);
            
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = EncryptionKey;
                aesAlg.IV = EncryptionIV;
                
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            
            return plaintext;
        }

        /// <summary>
        /// Encrypts only the password part in a connection string.
        /// 연결 문자열에서 비밀번호 부분만 암호화합니다.
        /// </summary>
        /// <param name="connectionString">Original connection string. 원본 연결 문자열</param>
        /// <returns>Connection string with only the password encrypted. 비밀번호만 암호화된 연결 문자열</returns>
        public static string EncryptPasswordInConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            // 비밀번호 부분 찾기
            int passwordStartIndex = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
            if (passwordStartIndex < 0)
                return connectionString; // 비밀번호 부분이 없으면 원본 그대로 반환
            
            passwordStartIndex += "Password=".Length;
            
            // 비밀번호 끝 부분 찾기 (세미콜론 또는 문자열 끝)
            int passwordEndIndex = connectionString.IndexOf(';', passwordStartIndex);
            if (passwordEndIndex < 0)
                passwordEndIndex = connectionString.Length;
            
            // 비밀번호 추출 및 암호화
            string password = connectionString.Substring(passwordStartIndex, passwordEndIndex - passwordStartIndex);
            string encryptedPassword = EncryptString(password);
            
            // 암호화된 비밀번호로 치환
            string result = connectionString.Substring(0, passwordStartIndex) + 
                           "ENC:" + encryptedPassword + 
                           (passwordEndIndex < connectionString.Length ? connectionString.Substring(passwordEndIndex) : "");
            
            return result;
        }

        /// <summary>
        /// Decrypts the encrypted password part in a connection string.
        /// 연결 문자열에서 암호화된 비밀번호 부분을 복호화합니다.
        /// </summary>
        /// <param name="connectionString">Connection string with encrypted password. 비밀번호가 암호화된 연결 문자열</param>
        /// <returns>Connection string with decrypted password. 복호화된 비밀번호를 포함한 연결 문자열</returns>
        public static string DecryptPasswordInConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            // 암호화된 비밀번호 부분 찾기
            int passwordStartIndex = connectionString.IndexOf("Password=ENC:", StringComparison.OrdinalIgnoreCase);
            if (passwordStartIndex < 0)
                return connectionString; // 암호화된 비밀번호가 없으면 원본 그대로 반환
            
            passwordStartIndex += "Password=ENC:".Length;
            
            // 비밀번호 끝 부분 찾기 (세미콜론 또는 문자열 끝)
            int passwordEndIndex = connectionString.IndexOf(';', passwordStartIndex);
            if (passwordEndIndex < 0)
                passwordEndIndex = connectionString.Length;
            
            // 암호화된 비밀번호 추출 및 복호화
            string encryptedPassword = connectionString.Substring(passwordStartIndex, passwordEndIndex - passwordStartIndex);
            string decryptedPassword = DecryptString(encryptedPassword);
            
            // 복호화된 비밀번호로 치환
            string result = connectionString.Substring(0, passwordStartIndex - "ENC:".Length) + 
                           decryptedPassword + 
                           (passwordEndIndex < connectionString.Length ? connectionString.Substring(passwordEndIndex) : "");
            
            return result;
        }

        /// <summary>
        /// Generates a new encryption key and IV and saves them to a file.
        /// 새로운 암호화 키와 IV를 생성하고 파일에 저장합니다.
        /// </summary>
        /// <param name="keyFilePath">File path to save the key. 키를 저장할 파일 경로</param>
        public static void GenerateAndSaveNewEncryptionKey(string keyFilePath)
        {
            using (Aes aesAlg = Aes.Create())
            {
                // Generate new key and IV
                // 새 키와 IV 생성
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();

                // Save key and IV to file
                // 키와 IV를 파일에 저장
                using (FileStream fs = new FileStream(keyFilePath, FileMode.Create))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(aesAlg.Key.Length);
                    bw.Write(aesAlg.Key);
                    bw.Write(aesAlg.IV.Length);
                    bw.Write(aesAlg.IV);
                }
                
                Console.WriteLine($"New encryption key saved to {keyFilePath}.\n새 암호화 키가 {keyFilePath}에 저장되었습니다.");
            }
        }

        /// <summary>
        /// Loads the encryption key and IV from a file.
        /// 파일에서 암호화 키와 IV를 로드합니다.
        /// </summary>
        /// <param name="keyFilePath">Key file path. 키 파일 경로</param>
        /// <returns>True if successful. 성공 여부</returns>
        public static bool LoadEncryptionKeyFromFile(string keyFilePath)
        {
            try
            {
                if (!File.Exists(keyFilePath))
                {
                    Console.WriteLine($"Key file not found: {keyFilePath}\n키 파일을 찾을 수 없습니다: {keyFilePath}");
                    return false;
                }

                byte[] key;
                byte[] iv;

                using (FileStream fs = new FileStream(keyFilePath, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int keyLength = br.ReadInt32();
                    key = br.ReadBytes(keyLength);
                    int ivLength = br.ReadInt32();
                    iv = br.ReadBytes(ivLength);
                }

                // To use loaded key and IV instead of hardcoded values,
                // call this method in the static constructor of this class and assign the loaded values to EncryptionKey and EncryptionIV fields.
                // 하드코딩된 값 대신 로드된 키와 IV를 사용하려면
                // 이 클래스의 static 생성자에서 이 메서드를 호출하고 
                // 로드된 값을 EncryptionKey와 EncryptionIV 필드에 할당해야 합니다.
                Console.WriteLine("Encryption key loaded successfully.\n암호화 키를 성공적으로 로드했습니다.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while loading key file: \n키 파일 로드 중 오류 발생: {ex.Message}");
                return false;
            }
        }
    }
}