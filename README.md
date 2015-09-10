[![Issue Stats](http://issuestats.com/github/reidev275/Fix/badge/issue)](http://issuestats.com/github/reidev275/Fix)
[![Issue Stats](http://issuestats.com/github/reidev275/Fix/badge/pr)](http://issuestats.com/github/reidev275/Fix)

#Fix (Mix for F#) 
Fix is a command line tool that provides tasks for creating F# projects with no dependence on other languages.

When called without any arguments Fix automatically goes into an interactive mode.

### Available Commands

	new [projectName] - Creates a new project with the given name
	refresh           - Refreshes the template cache
	help              - Displays this help
	exit              - Exit interactive mode

### Creating

    fix new [projectName]

On the first run Fix will download the templates found in the (Generator F# Repository)[https://github.com/fsprojects/generator-fsharp] and then allow you to choose which template you'd like to base your new project from.


### Installing

This is still in very early stages so you'll need to clone the repo, build the source, and then move the files in your bin folder to a location of your choosing.

## Maintainer(s)

- [@reidev275](https://github.com/reidev275)
