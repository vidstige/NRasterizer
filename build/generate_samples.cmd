@ECHO off
SET cli=dotnet run --project NRasterizer.CLI/ --configuration Release

REM Generate samples with the GDI+ rasterizer
ECHO Rendering samples with GDI+
%cli% gdi+ Fonts\CompositeMS.ttf samples\C.png C
%cli% gdi+ Fonts\segoeui.ttf samples\gdi\cefhijl.png cefhijl

ECHO Rendering sample with nrasterizer
REM Generate NRasterizer sample
%cli% nrasterizer Fonts\segoeui.ttf samples\clfx.png clfx
