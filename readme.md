# 파일 자동 정리 프로그램

이 프로젝트는 MS SQL 서버와 로컬 파일 시스템을 연동하여 파일을 관리하는 Windows 콘솔 애플리케이션입니다. 데이터베이스의 파일 목록과 로컬 파일을 비교하여 DB 유효 파일목록에 없는 파일을 임시 폴더로 이동하고, 일정 기간이 지난 파일을 자동으로 삭제하는 기능을 제공합니다.
[영어 버전(English Version)](README.en.md)
## 주요 기능

1. MS SQL 서버 연결 및 데이터베이스 조회
2. 파일 이름이 담긴 뷰(view) 조회
3. 로컬 드라이브의 특정 폴더에 있는 파일 목록 조회
4. 데이터베이스에 없는 파일을 임시 폴더로 이동
5. 임시 폴더로 이동한 파일명, 이동 일시를 DB 임시 테이블에 기록
6. 임시 테이블에서 이동 후 일정 기간(기본 30일)이 경과한 파일들 조회
7. 해당 파일을 임시 폴더에서 삭제하고, DB 임시 테이블에서도 기록 삭제
8. 앱 구성 파일(App.config)의 연결 문자열 보호를 위한 암호화 기능

## 시스템 요구사항

- Windows 운영체제
- .NET Framework 4.8
- MS SQL Server
- 적절한 파일 시스템 권한

## 솔루션 구성

솔루션은 다음과 같은 파일로 구성됩니다:

1. **FileManagerApp.cs**: 주 프로그램 로직
2. **App.config**: 애플리케이션 구성 파일
3. **ConfigEncryption.cs**: App.config 암호화 유틸리티
4. **ConfigEncryptionProgram.cs**: 구성 파일 암호화 관리 도구
5. **CustomEncryption.cs**: 커스텀 암호화 구현
6. **ConnectionStringManager.cs**: 연결 문자열 관리 클래스

## 설치 및 설정

1. 프로젝트를 빌드합니다.
2. App.config 파일에서 SQL 서버 연결 정보와 파일 경로를 설정합니다:

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

3. 데이터베이스에 필요한 뷰(view)가 존재하는지 확인합니다.
    - 뷰의 파일 이름은 "FileName" 열에 있어야 합니다.
4. (필요시) ConfigEncryptionProgram을 실행하여 연결 문자열을 암호화합니다.

## 사용 방법

### 기본 파일 관리 프로그램 실행

```
FileManagerApp.exe
```

프로그램은 다음 작업을 순차적으로 수행합니다:
1. 임시 테이블이 없으면 생성
2. DB에서 파일 목록 조회
3. 로컬 폴더에서 파일 목록 조회
4. DB에 없는 파일을 임시 폴더로 이동하고 DB에 기록
5. 지정된 일수가 경과한 임시 파일을 삭제하고 DB에서 기록 삭제

### 구성 파일 암호화 도구 실행

```
ConfigEncryptionProgram.exe
```

메뉴에서 원하는 암호화/복호화 작업을 선택합니다:
1. ConnectionStrings 섹션 암호화
2. ConnectionStrings 섹션 복호화
3. AppSettings 섹션 암호화
4. AppSettings 섹션 복호화

## 암호화 구현 종류

이 프로젝트는 세 가지 암호화 방식을 제공합니다:

1. **섹션 단위 암호화 (ConfigEncryption.cs)**
   - Windows DPAPI를 사용하여 App.config의 전체 섹션을 암호화합니다.
   - 사용자 또는 머신 계정에 종속적입니다.
   - `ConfigEncryptionProgram.exe`를 통해 관리할 수 있습니다.
2. **커스텀 암호화 (CustomEncryption.cs)**
   - AES 알고리즘을 사용한 자체 암호화 구현입니다.
   - 키를 파일로 관리할 수 있어 서버 간 이동이 가능합니다.
   - 프로그래밍 방식으로 암호화/복호화를 제어할 수 있습니다.
3. **비밀번호 부분 암호화 (ConnectionStringManager.cs)**
   - 연결 문자열에서 비밀번호 부분만 암호화합니다.
   - 구성 파일의 가독성을 유지하면서 보안을 강화할 수 있습니다.
   - 런타임에 자동으로 복호화되는 기능을 제공합니다.

## 보안 고려사항

- Windows DPAPI 방식은 동일 계정에서만 복호화가 가능합니다.
- 커스텀 암호화 키는 안전한 방식으로 관리해야 합니다.
- 프로덕션 환경에서는 하드코딩된 키를 사용하지 말고 외부 키 관리 시스템을 사용하세요.
- 서비스 계정으로 실행 시 적절한 암호화 방식을 선택하세요.

## 라이선스

이 프로젝트는 MIT 라이선스 하에 제공됩니다.