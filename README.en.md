# File Auto Cleaner Program

This project is a Windows console application that manages files by integrating MS SQL Server and the local file system. It compares the file list in the database with local files, moves files not present in the database to a temporary folder, and automatically deletes files that have passed a certain period.
[한국어 버전(Korean Version)](README.md)
## Main Features

1. Connect to MS SQL Server and query the database
2. Query a view containing file names
3. Retrieve file list from a specific local folder
4. Move files not in the database to a temp folder
5. Record file name and move date in a DB temp table
6. Query files in the temp table that have passed a certain period (default 30 days)
7. Delete those files from the temp folder and remove records from the DB temp table
8. Encrypt connection strings in the App.config for security

## System Requirements

- Windows OS
- .NET Framework 4.8.1
- MS SQL Server
- Appropriate file system permissions

## Solution Structure

The solution consists of the following files:
1. **FileManagerApp.cs**: Main program logic
2. **App.config**: Application configuration file
3. **ConfigEncryption.cs**: App.config encryption utility
4. **ConfigEncryptionProgram.cs**: Config file encryption management tool
5. **CustomEncryption.cs**: Custom encryption implementation
6. **ConnectionStringManager.cs**: Connection string management class

## Installation & Setup

1. Build the project.
2. Set SQL server connection info and file paths in App.config:

```xml
<connectionStrings>
    <add name="SqlConnection" connectionString="Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Integrated Security=True;" providerName="System.Data.SqlClient" />
</connectionStrings>
<appSettings>
    <add key="SourceFolderPath" value="C:\SourceFiles" />
    <add key="TempFolderPath" value="C:\TempFiles" />
    <add key="FileViewName" value="vwValidFiles" />
    <add key="TempTableName" value="TempMovedFiles" />
    <add key="DaysToKeep" value="30" />
</appSettings>
```
3. Ensure the required view exists in the database.
4. (If needed) Run ConfigEncryptionProgram to encrypt the connection string.

## Usage

### File Management Program

```
FileManagerApp.exe
```
The program performs the following steps in order:
1. Create temp table if it does not exist
2. Query file list from DB
3. Query file list from local folder
4. Move files not in DB to temp folder and record in DB
5. Delete temp files older than specified days and remove records from DB

### Config File Encryption Tool

```
ConfigEncryptionProgram.exe
```
Select the desired encryption/decryption operation from the menu:
1. Encrypt ConnectionStrings section
2. Decrypt ConnectionStrings section
3. Encrypt AppSettings section
4. Decrypt AppSettings section

## Types of Encryption Implemented

This project provides three types of encryption:
1. **Section-level encryption (ConfigEncryption.cs)**
   - Uses Windows DPAPI to encrypt entire sections of App.config.
   - Tied to user or machine account.
   - Managed via `ConfigEncryptionProgram.exe`.
2. **Custom encryption (CustomEncryption.cs)**
   - Custom AES-based encryption implementation.
   - Key can be managed via file, allowing server migration.
   - Programmatic control over encryption/decryption.
3. **Password-only encryption (ConnectionStringManager.cs)**
   - Encrypts only the password part of the connection string.
   - Maintains config readability while enhancing security.
   - Supports automatic decryption at runtime.

## Security Considerations

- Windows DPAPI can only be decrypted by the same account.
- Custom encryption keys should be managed securely.
- Do not use hardcoded keys in production; use an external key management system.
- Choose the appropriate encryption method for service accounts.

## License

This project is provided under the MIT License. 