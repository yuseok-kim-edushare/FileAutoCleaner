using System;
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileAutoCleaner;

namespace ConfigEncryptionTests
{
    [TestClass]
    public class ConfigEncryptionTests
    {
        private string tempConfigPath;

        [TestInitialize]
        public void Setup()
        {
            // Create a temp config file for each test
            tempConfigPath = Path.GetTempFileName() + ".config";
            File.WriteAllText(tempConfigPath, @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <connectionStrings>
    <add name=""TestConn"" connectionString=""Data Source=.;Initial Catalog=TestDb;User ID=sa;Password=Test123!"" providerName=""System.Data.SqlClient"" />
  </connectionStrings>
  <appSettings>
    <add key=""TestKey"" value=""TestValue"" />
  </appSettings>
</configuration>");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(tempConfigPath))
                File.Delete(tempConfigPath);
        }

        [TestMethod]
        public void SectionLevel_EncryptDecrypt_ConnectionStrings_PreservesValues()
        {
            // Encrypt
            Assert.IsTrue(ConfigEncryption.EncryptConnectionStrings(tempConfigPath));
            // Should now be encrypted (not human-readable)
            string encrypted = File.ReadAllText(tempConfigPath);
            Assert.IsTrue(encrypted.Contains("EncryptedData"));

            // Decrypt
            Assert.IsTrue(ConfigEncryption.DecryptConnectionStrings(tempConfigPath));
            string decrypted = File.ReadAllText(tempConfigPath);
            Assert.IsTrue(decrypted.Contains("TestConn"));
            Assert.IsTrue(decrypted.Contains("Password=Test123!"));
        }

        [TestMethod]
        public void SectionLevel_EncryptDecrypt_AppSettings_PreservesValues()
        {
            // Encrypt
            Assert.IsTrue(ConfigEncryption.EncryptAppSettings(tempConfigPath));
            string encrypted = File.ReadAllText(tempConfigPath);
            Assert.IsTrue(encrypted.Contains("EncryptedData"));

            // Decrypt
            Assert.IsTrue(ConfigEncryption.DecryptAppSettings(tempConfigPath));
            string decrypted = File.ReadAllText(tempConfigPath);
            Assert.IsTrue(decrypted.Contains("TestKey"));
            Assert.IsTrue(decrypted.Contains("TestValue"));
        }

        [TestMethod]
        public void ValueLevel_EncryptDecrypt_ConnectionStringPassword_PreservesOtherValues()
        {
            var mgr = new ConnectionStringManager(tempConfigPath);
            Assert.IsTrue(mgr.EncryptPasswordOnly("TestConn"));
            string afterEncrypt = File.ReadAllText(tempConfigPath);
            Assert.IsTrue(afterEncrypt.Contains("Password=ENC:"));
            Assert.IsTrue(afterEncrypt.Contains("TestConn"));

            // Always load from the temp config file
            var map = new ExeConfigurationFileMap { ExeConfigFilename = tempConfigPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            string encryptedConnStr = config.ConnectionStrings.ConnectionStrings["TestConn"]?.ConnectionString;

            string decrypted = CustomEncryption.DecryptPasswordInConnectionString(encryptedConnStr);
            Assert.IsTrue(decrypted.Contains("Password=Test123!"));
        }
    }
} 