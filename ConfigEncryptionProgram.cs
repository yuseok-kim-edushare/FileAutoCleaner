using System;

namespace FileAutoCleaner
{
    /// <summary>
    /// Standalone program for encrypting and decrypting configuration files.
    /// 구성 파일 암호화 및 복호화를 위한 독립 실행 프로그램
    /// </summary>
    class ConfigEncryptionProgram
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Configuration file encryption/decryption tool.\n구성 파일 암호화/복호화 도구");
                Console.WriteLine("===========================");
                Console.WriteLine();
                
                // Prompt for config file path
                Console.Write("Enter config file path (press Enter for default): ");
                string configPath = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(configPath))
                {
                    configPath = null;
                    Console.WriteLine("Using default config file.");
                }
                else
                {
                    Console.WriteLine($"Using config file: {configPath}");
                }
                Console.WriteLine();
                
                bool exitRequested = false;
                
                while (!exitRequested)
                {
                    Console.WriteLine("Select an operation:\n작업을 선택하세요:");
                    Console.WriteLine("1. Encrypt ConnectionStrings section\n1. ConnectionStrings 섹션 암호화");
                    Console.WriteLine("2. Decrypt ConnectionStrings section\n2. ConnectionStrings 섹션 복호화");
                    Console.WriteLine("3. Encrypt AppSettings section\n3. AppSettings 섹션 암호화");
                    Console.WriteLine("4. Decrypt AppSettings section\n4. AppSettings 섹션 복호화");
                    Console.WriteLine("5. Encrypt only password in a connection string\n5. 연결 문자열에서 비밀번호만 암호화");
                    Console.WriteLine("6. Decrypt only password in a connection string\n6. 연결 문자열에서 비밀번호만 복호화");
                    Console.WriteLine("7. Exit\n7. 종료");
                    Console.Write("Choice (1-7): 선택 (1-7): ");
                    
                    string choice = Console.ReadLine();
                    Console.WriteLine();
                    
                    // Print config file path before each operation
                    string pathMsg = configPath == null ? "(default config file)" : configPath;
                    Console.WriteLine($"[Config file: {pathMsg}]");
                    
                    switch (choice)
                    {
                        case "1":
                            ConfigEncryption.EncryptConnectionStrings(configPath);
                            break;
                            
                        case "2":
                            ConfigEncryption.DecryptConnectionStrings(configPath);
                            break;
                            
                        case "3":
                            ConfigEncryption.EncryptAppSettings(configPath);
                            break;
                            
                        case "4":
                            ConfigEncryption.DecryptAppSettings(configPath);
                            break;
                            
                        case "5":
                            Console.Write("Enter connection string name: ");
                            string encName = Console.ReadLine();
                            var encMgr = new ConnectionStringManager(configPath);
                            encMgr.EncryptPasswordOnly(encName);
                            break;
                            
                        case "6":
                            Console.Write("Enter connection string name: ");
                            string decName = Console.ReadLine();
                            var decMgr = new ConnectionStringManager(configPath);
                            // Decrypt password in connection string and update config
                            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings[decName]?.ConnectionString;
                            if (string.IsNullOrEmpty(connStr))
                            {
                                Console.WriteLine($"Cannot find connection string '{decName}'.\n연결 문자열 '{decName}'을 찾을 수 없습니다.");
                            }
                            else if (!connStr.Contains("Password=ENC:"))
                            {
                                Console.WriteLine("No encrypted password found in the connection string.\n연결 문자열에 암호화된 비밀번호가 없습니다.");
                            }
                            else
                            {
                                string decrypted = CustomEncryption.DecryptPasswordInConnectionString(connStr);
                                // Update config file with decrypted password
                                decMgr.GetType().GetMethod("UpdateConnectionStringInConfigFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                    .Invoke(decMgr, new object[] { decName, decrypted });
                                Console.WriteLine($"Password in connection string '{decName}' has been decrypted.\n연결 문자열 '{decName}'의 비밀번호가 복호화되었습니다.");
                            }
                            break;
                            
                        case "7":
                            exitRequested = true;
                            break;
                            
                        default:
                            Console.WriteLine("Invalid selection. Please try again.\n잘못된 선택입니다. 다시 시도하세요.");
                            break;
                    }
                    
                    Console.WriteLine();
                }
                
                Console.WriteLine("Exiting program.\n프로그램을 종료합니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: \n오류 발생: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("Press any key to exit...\n종료하려면 아무 키나 누르세요...");
            Console.ReadKey();
        }
    }
}