[![Issue Stats](http://issuestats.com/github/reidev275/Fix/badge/issue?style=flat-square)](http://issuestats.com/github/reidev275/Fix)
[![Issue Stats](http://issuestats.com/github/reidev275/Fix/badge/pr?style=flat-square)](http://issuestats.com/github/reidev275/Fix)
[![Build status](https://ci.appveyor.com/api/projects/status/94dsmj5nrnlbvykp?svg=true)](https://ci.appveyor.com/project/reidev275/fix/branch/master)

#Fix (Mix for F#)

[![Join the chat at https://gitter.im/reidev275/Fix](https://badges.gitter.im/reidev275/Fix.svg)](https://gitter.im/reidev275/Fix?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
Fix is a command line tool that provides tasks for creating F# projects with no dependence on other languages.

When called without any arguments Fix automatically goes into an interactive mode.

### Available Commands

     new [projectName] [projectDir] [templateName] [--no-paket]- Creates a new project with the given name, in given directory (relative to working directory) and given template. If parameters are not provided, program prompts user for them. Uses Paket, unless `--no-paket` flag is specified\n\
     file add [fileName] - Adds a file to the current folder and project.
     file remove [fileName] - Removes a file from the current folder and project.
     file list - List all files of the current project.
     reference add [reference] - Add a reference to the current project.
     reference remove [reference] - Remove a reference from the current project.
     reference list - List all references of the current project.
     update paket        - Updates Paket to latest version
     update fake         - Updates FAKE to latest version
     paket [args]        - Runs Paket with given arguments
     fake [args]         - Runs FAKE with given arguments
     refresh             - Refreshes the template cache
     help                - Displays this help
     exit                - Exit interactive mode

### Creating A project

    fix new [projectName]

On the first run Fix will download the templates found in the [Generator F# Repository](https://github.com/fsprojects/generator-fsharp) and then allow you to choose which template you'd like to base your new project from.

	C:\Dev>c:\tools\fix\fix.exe
	>
	Fix (Mix for F#)
	Available Commands:
	 new [projectName] - Creates a new project with the given name
	 refresh           - Refreshes the template cache
	 help              - Displays this help

	> new MySuaveProject
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
	Fixing template suave
	Creating C:\Dev\MySuaveProject
	Changing filenames from ApplicationName.* to MySuaveProject.*
	Changing namespace to MySuaveProject
	Changing guid to bb3d79ee-318d-435f-8807-54b2585b057c
	Done!

## Files within a project

### Adding a file to a project

	fix file add [fileName]

Adds a file to the current folder and project.  If more than one project file exists in the current directory you will be prompted which project you wish to add the file to.

### Removing a file from a project

	fix file remove [fileName]

Removes a file from the current folder and project.  If more than one project file exists in the current directory you will be prompted which project you wish to remove the file from.

### Installing

This is still in very early stages so you'll need to clone the repo, build the source, and then move the files in your bin folder to a location of your choosing.

## Maintainer(s)

- [@ReidNEvans](https://twitter.com/reidNEvans)
- [Krzysztof-Cieslak] (https://github.com/Krzysztof-Cieslak)
