@ECHO off
SET cli=NRasterizer.CLI\bin\Release\net45\NRasterizer.CLI.exe
ECHO %cli%
ECHO %cd%

DIR NRasterizer.CLI\bin
DIR NRasterizer.CLI\bin\Release
DIR NRasterizer.CLI\bin\Release\net45

%cli% gdi+ Fonts\CompositeMS.ttf samples\C.png C

ECHO Rendering sample with GDI+
REM Generate sample with the GDI+ rasterizer
%cli% gdi+ Fonts\segoeui.ttf samples\gdi\cefhijl.png cefhijl

ECHO Rendering sample with nrasterizer
REM Generate NRasterizer sample
%cli% nrasterizer Fonts\segoeui.ttf samples\clfx.png clfx
