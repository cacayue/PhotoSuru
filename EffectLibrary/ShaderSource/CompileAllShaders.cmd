@echo off

@set FXCTOOL=.\fxc.exe
@set BYTECODEDIR=..\ShaderBytecode

call :Compile bandedSwirl.fx
call :Compile brickMason.fx
call :Compile bulgeAndPinch.fx
call :Compile DirectionalBlur.fx
call :Compile magnify.fx
call :Compile monochrome.fx
call :Compile swirl.fx
call :Compile Ripple.fx
call :Compile InvertColor.fx
call :Compile SmoothMagnify.fx
call :Compile ZoomBlur.fx


goto Success

:Compile 
        %FXCTOOL% /nologo /T ps_2_0 %1 /E PS /Fo%BYTECODEDIR%\%1.ps
        exit/b 0

:Success
        echo Completed.
        exit/b 0
