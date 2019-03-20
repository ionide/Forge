[![Join the chat at https://gitter.im/fsharp-editing/Forge](https://badges.gitter.im/fsharp-editing/Forge.svg)](https://gitter.im/fsharp-editing/Forge?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


# Forge (F# Project Builder)

Forge is a command line tool that provides tasks for creating F# projects with no dependence on other languages. For more documentation please visit [our wiki pages](https://github.com/fsharp-editing/Forge/wiki)


### Installing

#### Via Scoop.sh (Windows)

You can install Forge via the [Scoop](http://scoop.sh/) package manager on Windows

    scoop install forge

#### Via Homebrew (OSX)

You can install Forge via the [Homebrew](http://brew.sh) package manager on OS X

    brew tap samritchie/forge && brew install forge

#### Via Linuxbrew (Linux)

You can install Forge via the [Linuxbrew](http://linuxbrew.sh/) package manager on Linux

    brew tap samritchie/forge && brew install forge

#### Other

You can download one of the releases found at https://github.com/fsharp-editing/Forge/releases

Alternately you can clone the repo, build the source, and then move the files in your bin folder to a location of your choosing.

##### On Linux:

One option is to build the project from source and create a symlink.

Choose an installation directory:

```bash
export BUILD_DIR=apps
mkdir -p $BUILD_DIR
cd $BUILD_DIR
```

Clone the repo and build the app:

```bash
git clone https://github.com/fsharp-editing/Forge
cd Forge
fake build
```

Choose a location for the executable:

```bash
export INSTALL_DIR=~/bin
mkdir -p $INSTALL_DIR
```

Ensure that the destination directory is on the user's `$PATH`.
For example, in the user's `.bashrc`, add the following line:

```
export PATH=~/bin:$PATH
```

Reload the `.bashrc` with `source` or `.`:

```bash
. ~/.bashrc
```

Create a symlink that points to the script, which runs executable.

```bash
ln -s `pwd`/temp/forge.sh $INSTALL_DIR/forge
```

## How to contribute

*Imposter syndrome disclaimer*: I want your help. No really, I do.

There might be a little voice inside that tells you you're not ready; that you need to do one more tutorial, or learn another framework, or write a few more blog posts before you can help me with this project.

I assure you, that's not the case.

This project has some clear Contribution Guidelines and expectations that you can [read here](https://github.com/ionide/forge/blob/master/CONTRIBUTING.md).

The contribution guidelines outline the process that you'll need to follow to get a patch merged. By making expectations and process explicit, I hope it will make it easier for you to contribute.

And you don't just have to write code. You can help out by writing documentation, tests, or even by giving feedback about this work. (And yes, that includes giving feedback about the contribution guidelines.)

Thank you for contributing!

## Contributing and copyright

The project is hosted on [GitHub](https://github.com/fsharp-editing/Forge) where you can [report issues](https://github.com/fsharp-editing/Forge/issues), fork
the project and submit pull requests.

The library is available under [unlicense](https://github.com/fsharp-editing/Forge/blob/master/LICENSE.md), which allows modification and redistribution for both commercial and non-commercial purposes.

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

## Maintainer(s)

- [Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)


#### Past Maintainer(s)

- [cloudRoutine](https://github.com/cloudRoutine/)
- [@ReidNEvans](https://twitter.com/reidNEvans)
