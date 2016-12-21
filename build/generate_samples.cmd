@ECHO off
NRasterizer.CLI\bin\Release\NRasterizer.CLI.exe gdi+ Fonts\CompositeMS.ttf samples\C.png C

ECHO Rendering sample with GDI+
REM Generate sample with the GDI+ rasterizer
NRasterizer.CLI\bin\Release\NRasterizer.CLI.exe gdi+ Fonts\segoeui.ttf samples\gdi\cefhijl.png cefhijl

ECHO Rendering sample with nrasterizer
REM Generate NRasterizer sample
NRasterizer.CLI\bin\Release\NRasterizer.CLI.exe nrasterizer Fonts\segoeui.ttf samples\clfx.png clfx
