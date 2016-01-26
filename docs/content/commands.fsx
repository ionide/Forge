

(**

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



*)