# How to contribute

Please take a moment to review this document in order to make the contribution process easy and effective for everyone involved!

## Using the issue tracker

Use the issues tracker for:

* [bug reports](#bug-reports)
* [feature requests](#feature-requests)
* [submitting pull requests](#pull-requests)

Personal support request should be discussed on [F# Software Foundation Slack](https://fsharp.org/guides/slack/).

## Bug reports

A bug is either a _demonstrable problem_ that is caused in Forge failing to provide the expected feature or indicate missing, unclear, or misleading documentation. Good bug reports are extremely helpful - thank you!
ą
Guidelines for bug reports:

1. **Use the GitHub issue search** &mdash; check if the issue has already been reported.

2. **Check if the issue has been fixed** &mdash; try to reproduce it using the `master` branch in the repository.

3. **Isolate and report the problem** &mdash; ideally create a reduced test case.

Please try to be as detailed as possible in your report. Include information about
your Operating System, as well as your `dotnet`. Please provide steps to
reproduce the issue as well as the outcome you were expecting! All these details
will help developers to fix any potential bugs.


## Feature requests

Feature requests are welcome and should be discussed on issue tracker. But take a moment to find
out whether your idea fits with the scope and aims of the project. It's up to *you*
to make a strong case to convince the community of the merits of this feature.
Please provide as much detail and context as possible.

## Pull requests

Good pull requests - patches, improvements, new features - are a fantastic
help. They should remain focused in scope and avoid containing unrelated
commits.

**IMPORTANT**: By submitting a patch, you agree that your work will be
licensed under the license used by the project.

If you have any large pull request in mind (e.g. implementing features,
refactoring code, etc), **please ask first** otherwise you risk spending
a lot of time working on something that the project's developers might
not want to merge into the project.

Please adhere to the coding conventions in the project (indentation,
accurate comments, etc.).

## How to build and test a local version of Forge

### Prerequisites

- [.NET Core 2.0](https://dotnet.microsoft.com/download)
- [FAKE 5](https://fake.build/)

### Building

Fork, from the github interface https://github.com/ionide/forge
 - if you don't use a certificate for committing to github:
```bash
git clone https://github.com/YOUR_GITHUB_USER/forge.git
```
 - if you use a certificate for github authentication:
```bash
git clone git@github.com:YOUR_GITHUB_USER/forge.git
```

#### First time build:
```bash
cd ionide-forge-fsharp
dotnet restore
fake build  # or build.cmd if your OS is Windows  (might need ./build Build here)
```

If `dotnet restore` gives the error `error MSB4126: The specified solution configuration "Debug|x64" is invalid`, there's a good chance you have the `Platform` environment variable set to "x64".  Unset the variable and try the restore command again.

You can also build project from VSCode with `Ctrl/Cmd + Shift + B`.

#### Running Forge:

```bash
dotnet run --project src/Forge
```

#### Running Tests

```
fake build -t Test
```

Or

```
dotnet run --project tests/Forge.Tests
```

#### Debugging

Debugging Forge or Forge.Tests is possible with VSCode - choose appropriate target in VSCode debug panel and press `F5`