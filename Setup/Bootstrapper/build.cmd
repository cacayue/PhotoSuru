@ECHO OFF
SETLOCAL

IF "%~1" == "" GOTO SYNTAX
SET INPUT_MSI=%~1
IF NOT EXIST "%INPUT_MSI%" GOTO MISSING_INPUT_MSI

IF "%~2" == "" GOTO SYNTAX
SET OUTPUT_EXE=%~2

IF NOT "%~3" == "" (
    SET PREREQ_MSI=%~1
    SET INPUT_MSI=%~2
    SET OUTPUT_EXE=%~3
    IF NOT EXIST "%~2" GOTO MISSING_INPUT_MSI
)

SET TEMPDIR=%TEMP%\tmp%RANDOM%
SET SRCDIR=%~dp0src

ECHO ON

mkdir "%TEMPDIR%"
mkdir "%TEMPDIR%\Payload\"

xcopy "%SRCDIR%"\. "%TEMPDIR%" /E /V /I /R /K /Y
copy /Y /B "%INPUT_MSI%" "%TEMPDIR%\Payload\Payload.msi"
echo TargetName=%TEMPDIR%\Setup.exe > "%TEMPDIR%\TargetNameSetup.exe.sed"

IF "%PREREQ_MSI%" == "" (
    copy /Y /B "%TEMPDIR%\PrologSetup.exe.sed" + "%TEMPDIR%\TargetNameSetup.exe.sed" + "%TEMPDIR%\EpilogSetup.exe.sed" ""%TEMPDIR%\Setup.exe.sed"
) else (
    copy /Y /B "%TEMPDIR%\PrologSetup.exe.sed" + "%TEMPDIR%\TargetNameSetup.exe.sed" + "%TEMPDIR%\EpilogSetup_Prereq.exe.sed" "%TEMPDIR%\Setup.exe.sed"
    copy /Y /B "%PREREQ_MSI%" "%TEMPDIR%\Payload\Prerequisite.msi"

    attrib -r %TEMPDIR%\Bootstrap\install.1033.xml
    copy /Y /B "%SRCDIR%\Bootstrap\install_prereq.1033.xml" "%TEMPDIR%\Bootstrap\install.1033.xml"
)

pushd "%TEMPDIR%" 
iexpress.exe /N Setup.exe.sed
popd

copy "%TEMPDIR%\Setup.exe" "%OUTPUT_EXE%"
mt -nologo -manifest "%TEMPDIR%\Setup.exe.manifest" -outputresource:"%OUTPUT_EXE%";#1

rd /s /q "%TEMPDIR%"
@ECHO OFF

GOTO EOF

:MISSING_INPUT_MSI
ECHO Cant find input MSI "%INPUT_MSI%"
GOTO SYNTAX

:MISSING_OUTPUT_EXE
ECHO Cant find output exe "%OUTPUT_EXE%"
GOTO SYNTAX

:SYNTAX
ECHO Syntax: build.cmd [prerequisite.msi] input.msi output.exe
GOTO EOF

:EOF