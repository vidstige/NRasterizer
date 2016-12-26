#!/bin/bash
PROGRAM="dotnet run --project NRasterizer.CLI/ --configuration Release"

$PROGRAM gdi+ Fonts/CompositeMS.ttf samples/C.png C

# Generate sample with the GDI+ rasterizer
$PROGRAM gdi+ Fonts/segoeui.ttf samples/gdi/cefhijl.png cefhijl

# Generate NRasterizer sample
$PROGRAM nrasterizer Fonts/segoeui.ttf samples/clfx.png clfx