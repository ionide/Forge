[![Issue Stats](http://issuestats.com/github/reidev275/Fix/badge/issue?style=flat-square)](http://issuestats.com/github/reidev275/Fix)
[![Issue Stats](http://issuestats.com/github/reidev275/Fix/badge/pr?style=flat-square)](http://issuestats.com/github/reidev275/Fix)
[![Build status](https://ci.appveyor.com/api/projects/status/mmy7xtj3ecy8il5e?svg=true)](https://ci.appveyor.com/project/reidev275/forge)
[![Build Status](https://travis-ci.org/fsprojects/Forge.svg?branch=master)](https://travis-ci.org/fsprojects/Forge)

[![Join the chat at https://gitter.im/fsprojects/Forge](https://badges.gitter.im/fsprojects/Forge.svg)](https://gitter.im/fsprojects/Forge?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


#Forge (F# Project Builder)

Forge is a command line tool that provides tasks for creating F# projects with no dependence on other languages.

When called without any arguments Forge automatically goes into an interactive mode.

### Available Commands

     new [--name <string>] [--dir <string>] [--template <string>] [--no-paket]- Creates a new project with the given name, in given directory (relative to working directory) and given template. If parameters are not provided, program prompts user for them. Uses Paket, unless `--no-paket` flag is specified\n\
     file add <string> - Adds a file to the current folder and project.
     file remove <string> - Removes a file from the current folder and project.
     file list - List all files of the current project.
	 file order <string> <string> - orders `file1` immediately before `file2` in the project.
     reference add <string> - Add a reference to the current project.
     reference remove <string> - Remove a reference from the current project.
     reference list - List all references of the current project.
     update paket        - Updates Paket to latest version
     update fake         - Updates FAKE to latest version
     paket <string>        - Runs Paket with given arguments
     fake <string>         - Runs FAKE with given arguments
     refresh             - Refreshes the template cache
     help                - Displays this help
     exit                - Exit interactive mode

### Creating a project

    new [--name <string>] [--dir <string>] [--template <string>] [--no-paket]

On the first run Forge will download the templates found in the [Generator F# Repository](https://github.com/fsprojects/generator-fsharp) and then allow you to choose which template you'd like to base your new project from.

	C:\Dev>c:\tools\forge\forge.exe
	>
	Forge (F# Project Builder)
	Available commands:
        new: Create new project
        file: Adds or removes file from current folder and project.
        reference: Adds or removes reference from current project.
        update: Updates Paket or FAKE
        paket: Runs Paket
        fake: Runs FAKE
        refresh: Refreshs the template cache
        help: Displays help
        exit: Exits interactive mode

	> new --name MySuaveProject --dir src
	Choose a template:
	 - aspwebapi2
	 - classlib
	 - console
	 - fslabbasic
	 - fslabjournal
	 - sln
	 - suave
	 - websharperserverclient
	 - websharperspa
	 - windows

	> suave
	Forging template suave
	Creating C:\Dev\MySuaveProject
	Changing filenames from ApplicationName.* to MySuaveProject.*
	Changing namespace to MySuaveProject
	Changing guid to bb3d79ee-318d-435f-8807-54b2585b057c
	Done!

Unless `--no-paket` flag is used, solution folder (folder in which `Forge` is running) will contain `.paket` folder and `paket.dependencies` and `paket.lock` file. Project folder will contain `paket.references` file.

Unless `--no-fake` flag is used, solution folder (folder in which `Forge` is running) will contain `build.fsx`, `build.cmd`, and `build.sh` files. It won't override previously existing files.

## Files within a project

### Adding a file to a project

	forge file add [fileName]

Adds a file to the current folder and project.  If more than one project file exists in the current directory you will be prompted which project you wish to add the file to.

### Removing a file from a project

	forge file remove [fileName]

Removes a file from the current folder and project.  If more than one project file exists in the current directory you will be prompted which project you wish to remove the file from.

### Installing

#### Via Scoop.sh (Windows)

You can install Forge via the [Scoop](http://scoop.sh/) package manager on Windows

    scoop install forge

#### Via Homebrew (OSX)

You can install Forge via the [Homebrew](http://brew.sh) package manager on OS X

    brew tap samritchie/forge && brew install forge

#### Other

You can download one of the releases found at https://github.com/fsprojects/Forge/releases

Alternately you can clone the repo, build the source, and then move the files in your bin folder to a location of your choosing.

## Maintainer(s)

- [@ReidNEvans](https://twitter.com/reidNEvans)
- [Krzysztof-Cieslak] (https://github.com/Krzysztof-Cieslak)
