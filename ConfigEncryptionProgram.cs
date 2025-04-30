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
                
                bool exitRequested = false;
                
                while (!exitRequested)
                {
                    Console.WriteLine("Select an operation:\n작업을 선택하세요:");
                    Console.WriteLine("1. Encrypt ConnectionStrings section\n1. ConnectionStrings 섹션 암호화");
                    Console.WriteLine("2. Decrypt ConnectionStrings section\n2. ConnectionStrings 섹션 복호화");
                    Console.WriteLine("3. Encrypt AppSettings section\n3. AppSettings 섹션 암호화");
                    Console.WriteLine("4. Decrypt AppSettings section\n4. AppSettings 섹션 복호화");
                    Console.WriteLine("5. Exit\n5. 종료");
                    Console.Write("Choice (1-5): 선택 (1-5): ");
                    
                    string choice = Console.ReadLine();
                    Console.WriteLine();
                    
                    switch (choice)
                    {
                        case "1":
                            ConfigEncryption.EncryptConnectionStrings();
                            break;
                            
                        case "2":
                            ConfigEncryption.DecryptConnectionStrings();
                            break;
                            
                        case "3":
                            ConfigEncryption.EncryptAppSettings();
                            break;
                            
                        case "4":
                            ConfigEncryption.DecryptAppSettings();
                            break;
                            
                        case "5":
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