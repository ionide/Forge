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
./build.sh
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

## Contributing and copyright

The project is hosted on [GitHub](https://github.com/fsharp-editing/Forge) where you can [report issues](https://github.com/fsharp-editing/Forge/issues), fork
the project and submit pull requests.

The library is available under [unlicense](https://github.com/fsharp-editing/Forge/blob/master/LICENSE.md), which allows modification and redistribution for both commercial and non-commercial purposes.

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

## Maintainer(s)

- [@ReidNEvans](https://twitter.com/reidNEvans)
- [Krzysztof-Cieslak](https://github.com/Krzysztof-Cieslak)
- [cloudRoutine](https://github.com/cloudRoutine/)
