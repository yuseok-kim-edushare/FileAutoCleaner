using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace FileAutoCleaner
{
    /// <summary>
    /// Connection string management class.
    /// 연결 문자열 관리 클래스
    /// </summary>
    public class ConnectionStringManager
    {
        private readonly string _configFilePath;

        /// <summary>
        /// Constructor
        /// 생성자
        /// </summary>
        /// <param name="configFilePath">Configuration file path (if null, uses the current app's config file). 구성 파일 경로 (null인 경우 현재 앱의 구성 파일 사용)</param>
        public ConnectionStringManager(string configFilePath = null)
        {
            _configFilePath = configFilePath ?? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        }

        /// <summary>
        /// Encrypts only the password in the connection string and saves it to the configuration file.
        /// 연결 문자열에서 비밀번호만 암호화하여 구성 파일에 저장합니다.
        /// </summary>
        /// <param name="connectionStringName">Connection string name. 연결 문자열 이름</param>
        /// <returns>True if successful. 성공 여부</returns>
        public bool EncryptPasswordOnly(string connectionStringName)
        {
            try
            {
                // Get the connection string
                // 연결 문자열 가져오기
                var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine($"Cannot find connection string '{connectionStringName}'.\n연결 문자열 '{connectionStringName}'을 찾을 수 없습니다.");
                    return false;
                }

                // Encrypt only the password
                // 비밀번호만 암호화
                string encryptedConnectionString = CustomEncryption.EncryptPasswordInConnectionString(connectionString);

                // Update with the encrypted connection string
                // 암호화된 연결 문자열로 업데이트
                UpdateConnectionStringInConfigFile(connectionStringName, encryptedConnectionString);
                
                Console.WriteLine($"Password in connection string '{connectionStringName}' has been encrypted.\n연결 문자열 '{connectionStringName}'의 비밀번호가 암호화되었습니다.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while encrypting connection string: \n연결 문자열 암호화 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds settings to automatically decrypt the connection string when used.
        /// 연결 문자열을 사용할 때 자동으로 복호화하기 위한 설정을 추가합니다.
        /// </summary>
        /// <returns>True if successful. 성공 여부</returns>
        public bool SetupConnectionStringDecryption()
        {
            try
            {
                // Load App.config as XML
                // App.config 파일을 XML로 로드
                XmlDocument doc = new XmlDocument();
                doc.Load(_configFilePath);

                // XPath navigation for updating settings
                // 설정 업데이트를 위한 XPath 탐색
                XmlNode configNode = doc.SelectSingleNode("//configuration");
                if (configNode == null)
                {
                    Console.WriteLine("Cannot find configuration node in config file.\n구성 파일에서 configuration 노드를 찾을 수 없습니다.");
                    return false;
                }

                // Check if connectionStringDecryption section exists
                // connectionStringDecryption 섹션이 있는지 확인
                XmlNode decryptionNode = doc.SelectSingleNode("//connectionStringDecryption");
                if (decryptionNode == null)
                {
                    // Add new section definition to configSections
                    // configSections에 새 섹션 정의 추가
                    XmlNode configSectionsNode = doc.SelectSingleNode("//configSections");
                    if (configSectionsNode == null)
                    {
                        configSectionsNode = doc.CreateElement("configSections");
                        configNode.PrependChild(configSectionsNode);
                    }

                    // Add section definition
                    // 섹션 정의 추가
                    XmlElement sectionElement = doc.CreateElement("section");
                    sectionElement.SetAttribute("name", "connectionStringDecryption");
                    sectionElement.SetAttribute("type", "FileManagerApp.ConnectionStringDecryptionSection, FileManagerApp");
                    configSectionsNode.AppendChild(sectionElement);

                    // Add connectionStringDecryption section
                    // connectionStringDecryption 섹션 추가
                    XmlElement decryptionElement = doc.CreateElement("connectionStringDecryption");
                    decryptionElement.SetAttribute("enabled", "true");
                    configNode.AppendChild(decryptionElement);

                    // Save changes
                    // 변경사항 저장
                    doc.Save(_configFilePath);
                    Console.WriteLine("Automatic connection string decryption setting added.\n연결 문자열 자동 복호화 설정이 추가되었습니다.");
                }
                else
                {
                    // If already exists, enable it
                    // 이미 있는 경우 활성화
                    XmlElement decryptionElement = (XmlElement)decryptionNode;
                    decryptionElement.SetAttribute("enabled", "true");
                    doc.Save(_configFilePath);
                    Console.WriteLine("Automatic connection string decryption enabled.\n연결 문자열 자동 복호화가 활성화되었습니다.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while setting up connection string decryption: \n연결 문자열 복호화 설정 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the connection string in the configuration file.
        /// 구성 파일의 연결 문자열을 업데이트합니다.
        /// </summary>
        /// <param name="connectionStringName">Connection string name. 연결 문자열 이름</param>
        /// <param name="connectionString">New connection string value. 새 연결 문자열 값</param>
        private void UpdateConnectionStringInConfigFile(string connectionStringName, string connectionString)
        {
            // Load the configuration file as XML
            // 구성 파일을 XML로 로드
            XmlDocument doc = new XmlDocument();
            doc.Load(_configFilePath);

            // Find the connectionStrings section
            // connectionStrings 섹션 찾기
            XmlNode connectionStringsNode = doc.SelectSingleNode("//connectionStrings");
            if (connectionStringsNode == null)
            {
                throw new InvalidOperationException("Cannot find connectionStrings section in config file.\n구성 파일에서 connectionStrings 섹션을 찾을 수 없습니다.");
            }

            // Find the connection string with the specified name
            // 지정된 이름의 연결 문자열 찾기
            XmlNode addNode = connectionStringsNode.SelectSingleNode($"add[@name='{connectionStringName}']");
            if (addNode == null)
            {
                throw new InvalidOperationException($"Cannot find connection string '{connectionStringName}' in config file.\n연결 문자열 '{connectionStringName}'을 찾을 수 없습니다.");
            }

            // Update the connection string
            // 연결 문자열 업데이트
            XmlElement addElement = (XmlElement)addNode;
            addElement.SetAttribute("connectionString", connectionString);

            // Save changes
            // 변경 사항 저장
            doc.Save(_configFilePath);
        }
    }

    /// <summary>
    /// Configuration section class for connection string decryption.
    /// 연결 문자열 복호화를 위한 구성 섹션 클래스
    /// </summary>
    public class ConnectionStringDecryptionSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled", DefaultValue = "false", IsRequired = false)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }
    }

    /// <summary>
    /// Provider for automatically decrypting encrypted connection strings at usage time.
    /// 암호화된 연결 문자열을 사용 시점에 자동으로 복호화하는 프로바이더
    /// </summary>
    public class ConnectionStringDecryptionProvider
    {
        // Automatically decrypt connection string when used
        // 연결 문자열 사용 시 자동으로 복호화
        public static string GetDecryptedConnectionString(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            // Check if the connection string contains an encrypted password and decrypt
            // 연결 문자열에 암호화된 비밀번호가 있는지 확인하고 복호화
            if (connectionString.Contains("Password=ENC:"))
            {
                return CustomEncryption.DecryptPasswordInConnectionString(connectionString);
            }

            return connectionString;
        }
    }
}