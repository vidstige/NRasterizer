#!/bin/bash

PROGRAM=NRasterizer.CLI/bin/Release/NRasterizer.CLI.exe

# Generate sample with the GDI+ rasterizer
mono $PROGRAM gdi+ Sample/segoeui.ttf samples/gdi/cefhijl.png cefhijl

# Generate NRasterizer sample
mono $PROGRAM gdi+ Sample/segoeui.ttf samples/F.png F
