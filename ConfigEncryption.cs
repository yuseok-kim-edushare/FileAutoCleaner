using System;
using System.Configuration;
using System.Reflection;

namespace FileAutoCleaner
{
    /// <summary>
    /// Utility class for encrypting and decrypting sections of a configuration file.
    /// 구성 파일의 섹션을 암호화하고 복호화하는 유틸리티 클래스
    /// </summary>
    public class ConfigEncryption
    {
        /// <summary>
        /// Encrypts the connectionStrings section.
        /// connectionStrings 섹션을 암호화합니다.
        /// </summary>
        /// <param name="configPath">Path to the configuration file to encrypt (if null, uses the current app's config file). 암호화할 구성 파일의 경로 (null인 경우 현재 앱의 구성 파일 사용)</param>
        /// <returns>True if encryption succeeded. 암호화 성공 여부</returns>
        public static bool EncryptConnectionStrings(string configPath = null)
        {
            try
            {
                // If config path is not specified, use the current app's config file.
                // 구성 파일 경로가 지정되지 않은 경우 현재 앱의 구성 파일 사용
                Configuration config = OpenConfiguration(configPath);
                
                // Get the connectionStrings section
                // connectionStrings 섹션 가져오기
                ConfigurationSection section = config.GetSection("connectionStrings");
                
                if (section != null && !section.SectionInformation.IsProtected)
                {
                    // Encrypt the section using DPAPI (current user or machine account)
                    // DPAPI를 사용하여 섹션 암호화 (현재 사용자 또는 머신 계정)
                    section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    section.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("connectionStrings");
                    
                    Console.WriteLine("connectionStrings section encrypted successfully.\nconnectionStrings 섹션이 성공적으로 암호화되었습니다.");
                    return true;
                }
                else if (section != null && section.SectionInformation.IsProtected)
                {
                    Console.WriteLine("connectionStrings section is already encrypted.\nconnectionStrings 섹션은 이미 암호화되어 있습니다.");
                    return true;
                }
                else
                {
                    Console.WriteLine("connectionStrings section not found.\nconnectionStrings 섹션을 찾을 수 없습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while encrypting connectionStrings section: {ex.Message}\nconnectionStrings 섹션 암호화 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decrypts the connectionStrings section.
        /// connectionStrings 섹션의 암호화를 해제합니다.
        /// </summary>
        /// <param name="configPath">Path to the configuration file to decrypt (if null, uses the current app's config file). 복호화할 구성 파일의 경로 (null인 경우 현재 앱의 구성 파일 사용)</param>
        /// <returns>True if decryption succeeded. 복호화 성공 여부</returns>
        public static bool DecryptConnectionStrings(string configPath = null)
        {
            try
            {
                // If config path is not specified, use the current app's config file.
                // 구성 파일 경로가 지정되지 않은 경우 현재 앱의 구성 파일 사용
                Configuration config = OpenConfiguration(configPath);
                
                // Get the connectionStrings section
                // connectionStrings 섹션 가져오기
                ConfigurationSection section = config.GetSection("connectionStrings");
                
                if (section != null && section.SectionInformation.IsProtected)
                {
                    // Decrypt the section
                    // 섹션 암호화 해제
                    section.SectionInformation.UnprotectSection();
                    section.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("connectionStrings");
                    
                    Console.WriteLine("connectionStrings section decrypted successfully.\nconnectionStrings 섹션이 성공적으로 복호화되었습니다.");
                    return true;
                }
                else if (section != null && !section.SectionInformation.IsProtected)
                {
                    Console.WriteLine("connectionStrings section is not encrypted.\nconnectionStrings 섹션은 암호화되어 있지 않습니다.");
                    return true;
                }
                else
                {
                    Console.WriteLine("connectionStrings section not found.\nconnectionStrings 섹션을 찾을 수 없습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while decrypting connectionStrings section: {ex.Message}\nconnectionStrings 섹션 복호화 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Encrypts the appSettings section.
        /// appSettings 섹션을 암호화합니다.
        /// </summary>
        /// <param name="configPath">Path to the configuration file to encrypt (if null, uses the current app's config file). 암호화할 구성 파일의 경로 (null인 경우 현재 앱의 구성 파일 사용)</param>
        /// <returns>True if encryption succeeded. 암호화 성공 여부</returns>
        public static bool EncryptAppSettings(string configPath = null)
        {
            try
            {
                // If config path is not specified, use the current app's config file.
                // 구성 파일 경로가 지정되지 않은 경우 현재 앱의 구성 파일 사용
                Configuration config = OpenConfiguration(configPath);
                
                // Get the appSettings section
                // appSettings 섹션 가져오기
                ConfigurationSection section = config.GetSection("appSettings");
                
                if (section != null && !section.SectionInformation.IsProtected)
                {
                    // Encrypt the section using DPAPI (current user or machine account)
                    // DPAPI를 사용하여 섹션 암호화 (현재 사용자 또는 머신 계정)
                    section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    section.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    
                    Console.WriteLine("appSettings section encrypted successfully.\nappSettings 섹션이 성공적으로 암호화되었습니다.");
                    return true;
                }
                else if (section != null && section.SectionInformation.IsProtected)
                {
                    Console.WriteLine("appSettings section is already encrypted.\nappSettings 섹션은 이미 암호화되어 있습니다.");
                    return true;
                }
                else
                {
                    Console.WriteLine("appSettings section not found.\nappSettings 섹션을 찾을 수 없습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while encrypting appSettings section: {ex.Message}\nappSettings 섹션 암호화 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decrypts the appSettings section.
        /// appSettings 섹션의 암호화를 해제합니다.
        /// </summary>
        /// <param name="configPath">Path to the configuration file to decrypt (if null, uses the current app's config file). 복호화할 구성 파일의 경로 (null인 경우 현재 앱의 구성 파일 사용)</param>
        /// <returns>True if decryption succeeded. 복호화 성공 여부</returns>
        public static bool DecryptAppSettings(string configPath = null)
        {
            try
            {
                // If config path is not specified, use the current app's config file.
                // 구성 파일 경로가 지정되지 않은 경우 현재 앱의 구성 파일 사용
                Configuration config = OpenConfiguration(configPath);
                
                // Get the appSettings section
                // appSettings 섹션 가져오기
                ConfigurationSection section = config.GetSection("appSettings");
                
                if (section != null && section.SectionInformation.IsProtected)
                {
                    // Decrypt the section
                    // 섹션 암호화 해제
                    section.SectionInformation.UnprotectSection();
                    section.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    
                    Console.WriteLine("appSettings section decrypted successfully.\nappSettings 섹션이 성공적으로 복호화되었습니다.");
                    return true;
                }
                else if (section != null && !section.SectionInformation.IsProtected)
                {
                    Console.WriteLine("appSettings section is not encrypted.\nappSettings 섹션은 암호화되어 있지 않습니다.");
                    return true;
                }
                else
                {
                    Console.WriteLine("appSettings section not found.\nappSettings 섹션을 찾을 수 없습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while decrypting appSettings section: {ex.Message}\nappSettings 섹션 복호화 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens the configuration file at the specified path or the current app's config file.
        /// 지정된 경로의 구성 파일을 열거나 현재 앱의 구성 파일을 엽니다.
        /// </summary>
        /// <param name="configPath">Configuration file path (if null, uses the current app's config file). 구성 파일 경로 (null인 경우 현재 앱의 구성 파일 사용)</param>
        /// <returns>The opened configuration object. 열린 구성 객체</returns>
        private static Configuration OpenConfiguration(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                // Open the current executable's configuration
                // 현재 실행 파일의 구성 열기
                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            else
            {
                // Open the configuration file at the specified path
                // 지정된 경로의 구성 파일 열기
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configPath;
                return ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            }
        }
    }
}