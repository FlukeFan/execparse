
SET PackageFolder=ExecParse1.0.0.0

C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\MSBuild /p:Configuration=Release /t:Clean
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\MSBuild /p:Configuration=Release /t:Build

RMDIR /Q /S %PackageFolder%
MKDIR %PackageFolder%

COPY licence.txt %PackageFolder%
COPY bin\Release\ExecParse.dll %PackageFolder%
