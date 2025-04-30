using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Configuration;

namespace FileAutoCleaner
{
    class Program
    {
        // Load configuration variables at program start
        // 설정 변수들을 프로그램 시작할 때 로드
        private static string ConnectionString;
        private static string SourceFolderPath;
        private static string TempFolderPath;
        private static string FileViewName;
        private static string TempTableName;
        private static int DaysToKeep;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("파일 관리 프로그램을 시작합니다.");
                
                // Load settings (encrypted settings are also automatically decrypted)
                // 설정 로드 (암호화된 설정도 자동으로 복호화됨)
                LoadConfiguration();
                
                // 1. Create temp table if it does not exist
                // 1. 임시 테이블이 없으면 생성
                EnsureTempTableExists();
                
                // 2. Retrieve file list from DB
                // 2. DB에서 파일 목록 조회
                List<string> dbFiles = GetFileListFromDatabase();
                Console.WriteLine($"Retrieved {dbFiles.Count} file records from database.\n데이터베이스에서 {dbFiles.Count}개 파일 정보를 조회했습니다.");
                
                // 3. Retrieve file list from local folder
                // 3. 로컬 폴더에서 파일 목록 조회
                List<string> localFiles = GetLocalFileList();
                Console.WriteLine($"Found {localFiles.Count} files in local folder.\n로컬 폴더에서 {localFiles.Count}개 파일을 찾았습니다.");
                
                // 4. Find files not in DB and move to temp folder
                // 4. DB에 없는 파일 찾기 및 임시 폴더로 이동
                List<string> filesToMove = localFiles.Except(dbFiles, StringComparer.OrdinalIgnoreCase).ToList();
                MoveFilesToTempFolder(filesToMove);
                
                // 5. Delete temp files older than 30 days
                // 5. 30일 이상 지난 임시 파일 삭제
                DeleteOldTempFiles();
                
                Console.WriteLine("File management tasks completed.\n파일 관리 작업이 완료되었습니다.");
                Console.WriteLine("Press any key to exit...\n프로그램을 종료하려면 아무 키나 누르세요...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: \n오류 발생: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Press any key to exit...\n프로그램을 종료하려면 아무 키나 누르세요...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Load settings from encrypted configuration file
        /// 암호화된 구성 파일에서 설정 로드
        /// </summary>
        private static void LoadConfiguration()
        {
            try
            {
                // Even if ConnectionStrings section is encrypted, ConfigurationManager automatically decrypts it
                // ConnectionStrings 섹션은 암호화되어 있더라도 ConfigurationManager가 자동으로 복호화함
                ConnectionString = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
                
                // AppSettings values are also automatically decrypted
                // AppSettings 값도 마찬가지로 자동 복호화됨
                SourceFolderPath = ConfigurationManager.AppSettings["SourceFolderPath"];
                TempFolderPath = ConfigurationManager.AppSettings["TempFolderPath"];
                FileViewName = ConfigurationManager.AppSettings["FileViewName"];
                TempTableName = ConfigurationManager.AppSettings["TempTableName"];
                
                // If DaysToKeep is missing or not a number, use default value 30
                // DaysToKeep 값이 없거나 숫자가 아닐 경우 기본값 30 사용
                if (!int.TryParse(ConfigurationManager.AppSettings["DaysToKeep"], out DaysToKeep))
                {
                    DaysToKeep = 30;
                    Console.WriteLine("DaysToKeep setting is invalid, using default value 30.\nDaysToKeep 설정값이 유효하지 않아 기본값 30을 사용합니다.");
                }
                
                Console.WriteLine("Configuration settings loaded successfully.\n구성 설정을 성공적으로 로드했습니다.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while loading settings: \n설정 로드 중 오류 발생: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create temp table if it does not exist
        /// 임시 테이블이 없으면 생성하는 메서드
        /// </summary>
        private static void EnsureTempTableExists()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                
                // Check if table exists
                // 테이블 존재 여부 확인
                string checkTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName)
                    BEGIN
                        CREATE TABLE [dbo].[" + TempTableName + @"] (
                            [Id] INT IDENTITY(1,1) PRIMARY KEY,
                            [FileName] NVARCHAR(255) NOT NULL,
                            [MovedDate] DATETIME NOT NULL DEFAULT GETDATE()
                        )
                    END";
                
                using (SqlCommand command = new SqlCommand(checkTableQuery, connection))
                {
                    command.Parameters.AddWithValue("@TableName", TempTableName);
                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine($"Temp table {TempTableName} check complete.\n임시 테이블 {TempTableName} 확인 완료");
        }

        /// <summary>
        /// Retrieve file list from database
        /// 데이터베이스에서 파일 목록을 조회하는 메서드
        /// </summary>
        private static List<string> GetFileListFromDatabase()
        {
            List<string> fileList = new List<string>();
            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                
                string query = $"SELECT * FROM {FileViewName}";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Adjust file name column to match actual view
                        // 파일 이름 컬럼명은 실제 뷰에 맞게 수정 필요
                        string fileName = reader["FileName"].ToString();
                        fileList.Add(fileName);
                    }
                }
            }
            
            return fileList;
        }

        /// <summary>
        /// Retrieve file list from local folder
        /// 로컬 폴더에서 파일 목록을 조회하는 메서드
        /// </summary>
        private static List<string> GetLocalFileList()
        {
            if (!Directory.Exists(SourceFolderPath))
            {
                throw new DirectoryNotFoundException($"Cannot find specified source folder: {SourceFolderPath}\n지정된 소스 폴더를 찾을 수 없습니다: {SourceFolderPath}");
            }
            
            return Directory.GetFiles(SourceFolderPath)
                .Select(Path.GetFileName)
                .ToList();
        }

        /// <summary>
        /// Move files to temp folder and record in DB
        /// 파일을 임시 폴더로 이동하고 DB에 기록하는 메서드
        /// </summary>
        private static void MoveFilesToTempFolder(List<string> files)
        {
            if (files.Count == 0)
            {
                Console.WriteLine("No files to move.\n이동할 파일이 없습니다.");
                return;
            }
            
            // Create temp folder if it does not exist
            // 임시 폴더가 없으면 생성
            if (!Directory.Exists(TempFolderPath))
            {
                Directory.CreateDirectory(TempFolderPath);
                Console.WriteLine($"Temp folder created: {TempFolderPath}\n임시 폴더 생성: {TempFolderPath}");
            }
            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                
                // Commit transaction
                // 트랜잭션 커밋
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (string fileName in files)
                        {
                            string sourcePath = Path.Combine(SourceFolderPath, fileName);
                            string destPath = Path.Combine(TempFolderPath, fileName);
                            
                            // Move file
                            // 파일 이동
                            File.Move(sourcePath, destPath);
                            
                            // Record in DB
                            // DB에 기록
                            string insertQuery = $"INSERT INTO {TempTableName} (FileName, MovedDate) VALUES (@FileName, GETDATE())";
                            using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@FileName", fileName);
                                command.ExecuteNonQuery();
                            }
                        }
                        
                        // Commit transaction
                        // 트랜잭션 커밋
                        transaction.Commit();
                        Console.WriteLine($"Moved {files.Count} files to temp folder and recorded in DB.\n{files.Count}개 파일을 임시 폴더로 이동하고 DB에 기록했습니다.");
                    }
                    catch (Exception ex)
                    {
                        // Rollback on error
                        // 오류 발생 시 롤백
                        transaction.Rollback();
                        throw new Exception($"Error occurred while moving files: {ex.Message}\n파일 이동 중 오류 발생: {ex.Message}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Delete temp files that have passed the specified number of days
        /// 이동 후 지정된 일수가 경과한 임시 파일을 삭제하는 메서드
        /// </summary>
        private static void DeleteOldTempFiles()
        {
            List<OldTempFile> oldFiles = new List<OldTempFile>();
            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                
                // Query files older than specified period from temp table
                // 임시 테이블에서 일정 기간 이상 경과한 파일 조회
                string query = $@"
                    SELECT Id, FileName, MovedDate 
                    FROM {TempTableName} 
                    WHERE DATEDIFF(DAY, MovedDate, GETDATE()) >= @DaysToKeep";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DaysToKeep", DaysToKeep);
                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oldFiles.Add(new OldTempFile
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FileName = reader["FileName"].ToString(),
                                MovedDate = Convert.ToDateTime(reader["MovedDate"])
                            });
                        }
                    }
                }
                
                if (oldFiles.Count == 0)
                {
                    Console.WriteLine($"No old files to delete (based on {DaysToKeep} days).\n삭제할 오래된 파일이 없습니다({DaysToKeep}일 기준).");
                    return;
                }
                
                // Delete files and DB records in transaction
                // 트랜잭션으로 파일 삭제 및 DB 레코드 삭제
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var file in oldFiles)
                        {
                            string filePath = Path.Combine(TempFolderPath, file.FileName);
                            
                            // Delete file if exists
                            // 파일이 존재하면 삭제
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            
                            // Delete DB record
                            // DB 레코드 삭제
                            string deleteQuery = $"DELETE FROM {TempTableName} WHERE Id = @Id";
                            using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Id", file.Id);
                                command.ExecuteNonQuery();
                            }
                        }
                        
                        // Commit transaction
                        // 트랜잭션 커밋
                        transaction.Commit();
                        Console.WriteLine($"Deleted {oldFiles.Count} old files and related records.\n{oldFiles.Count}개의 오래된 파일 및 관련 레코드를 삭제했습니다.");
                    }
                    catch (Exception ex)
                    {
                        // Rollback on error
                        // 오류 발생 시 롤백
                        transaction.Rollback();
                        throw new Exception($"Error occurred while deleting old files: {ex.Message}\n오래된 파일 삭제 중 오류 발생: {ex.Message}", ex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Class to store information about old temp files
    /// 오래된 임시 파일 정보를 저장하는 클래스
    /// </summary>
    class OldTempFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime MovedDate { get; set; }
    }
}