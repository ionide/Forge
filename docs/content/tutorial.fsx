(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../temp/bin"

(**
Getting started
======================

Creating a project
------------------

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


*)
