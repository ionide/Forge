#!/bin/bash
# User just ran a symbolic link which points to real Forge.exe
# $0 is an absolute path of this symboliklink
# $* stands for all arguments passed to the command
dotnet "Bin/Forge.dll" $*
