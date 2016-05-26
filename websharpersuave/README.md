# <%= namespace %>

A [websharper.suave](https://github.com/intellifactory/websharper.suave)
application. __Only tested with Paket__ - NuGet support will likely require
adding dependencies to the project file.

## Logging

Uncomment the debugLogger, and replace the defaultLogger.
Very verbose output, more log levels are available.

## Build

### Linux / MONO

		$ ./build.sh
		$ mono ./build/<%= namespace %>.exe

