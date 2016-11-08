[![Build status](https://ci.appveyor.com/api/projects/status/dbrajqyaiewsi9ex?svg=true)](https://ci.appveyor.com/project/fsharpediting/forge)
[![Build Status](https://travis-ci.org/fsharp-editing/Forge.svg?branch=master)](https://travis-ci.org/fsharp-editing/Forge)
[![Join the chat at https://gitter.im/fsharp-editing/Forge](https://badges.gitter.im/fsharp-editing/Forge.svg)](https://gitter.im/fsprojects/Forge?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


#Forge (F# Project Builder)

Forge is a command line tool that provides tasks for creating F# projects with no dependence on other languages.

When called without any arguments Forge automatically goes into an interactive mode.

### Available Commands

    Available parameters:
        new: <project|file> Create new file or project
        add: <file|reference|project> Adds file, reference or project reference
        remove: <file|folder|reference|project> Removes file, folder, reference or project reference
        rename: <project|file> Renames file or project
        move: <file> Move the file within the project hierarchy
        list: <files|projects|references|projectReferences|templates|gac> List files, project in solution, references, project references, avaliable templates or libraries installed in gac
        update: <paket|fake> Updates Paket or FAKE
        paket: Runs Paket
        fake: Runs FAKE
        refresh: Refreshes the template cache
        exit [quit|-q]: Exits interactive mode
        --help [-h|/h|/help|/?]: display this list of options.

### Creating a project

    new project [--name <string>] [--dir <string>] [--template <string>] [--no-paket]

On the first run Forge will download the templates found in the [Forge Repository](https://github.com/fsprojects/forge/tree/templates) and then allow you to choose which template you'd like to base your new project from.

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

	> new project --name MySuaveProject --dir src
	Choose a template:
	 - aspwebapi2
     - classlib
     - console
     - fslabbasic
     - fslabjournal
     - pcl259
     - servicefabrichost
     - servicefabricsuavestateless
     - sln
     - suave
     - suaveazurebootstrapper
     - websharperserverclient
     - websharperspa
     - websharpersuave
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


### Installing

#### Via Scoop.sh (Windows)

You can install Forge via the [Scoop](http://scoop.sh/) package manager on Windows

    scoop install forge

#### Via Homebrew (OSX)

You can install Forge via the [Homebrew](http://brew.sh) package manager on OS X

    brew tap samritchie/forge && brew install forge

#### Other

You can download one of the releases found at https://github.com/fsharp-editing/Forge/releases

Alternately you can clone the repo, build the source, and then move the files in your bin folder to a location of your choosing.

## Maintainer(s)

- [@ReidNEvans](https://twitter.com/reidNEvans)
- [Krzysztof-Cieslak] (https://github.com/Krzysztof-Cieslak)
- [cloudRoutine](https://github.com/cloudRoutine/)