

@ECHO OFF

IF '%MongoDatabase__ConnectionString%'=='' SET MongoDatabase__ConnectionString=mongodb://localhost:27017

@ECHO ON
dotnet run ^
--project Tools\Example.Dataloader.Cli ^
--configuration Release ^
--no-build ^
--no-restore ^
-- ^
--SourcePath=.\Conf\MongoDb\SampleData ^
--Action=DropCollectionAndImport ^
--ConnectionString=%MongoDatabase__ConnectionString% ^
--DatabaseName=example

@ECHO OFF

REM dotnet.exe run ^
REM --project .\tools\Example.Dataloader.Cli ^
REM --configuration Release ^
REM --no-build ^
REM --no-restore ^
REM --nologo ^
REM -- ^
REM --SourcePath=.\Conf\MongoDb\SampleData ^
REM --Action=Import ^
REM --DatabaseName=exampleX


REM --no-build --no-restore --nologo ^