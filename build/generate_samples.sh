#!/bin/bash

PROGRAM=NRasterizer.CLI/bin/Release/NRasterizer.CLI.exe

# Generate sample with the GDI+ rasterizer
mono $PROGRAM gdi+ Fonts/segoeui.ttf samples/gdi/cefhijl.png cefhijl

# Generate NRasterizer sample
mono $PROGRAM nrasterizer Fonts/segoeui.ttf samples/clfx.png clfx
