// --------------------------------------------------------------------------------------
// Runner for processing FsLab Journals - this gets executed when you run the project.
// All work is deleagetd to "build.fsx" FAKE script - we just run it using cmd/bash.
// --------------------------------------------------------------------------------------
open System.IO
open System.Diagnostics

[<EntryPoint>]
let main argv =
  let (@@) p1 p2 = Path.Combine(p1, p2)
  let currentDir = __SOURCE_DIRECTORY__

  // If the build file is empty (project was just created)
  // we copy the latest version from the FsLab.Runner package
  if File.ReadAllBytes(currentDir @@ "build.fsx").Length < 10 then
    for f in Directory.GetFiles(currentDir @@ "<%= packagesPath %>/FsLab.Runner/tools") do
      File.Copy(f, currentDir @@ Path.GetFileName(f), true)

  // Start the build script using the appropriate command
  let info =
    if System.Type.GetType("Mono.Runtime") <> null then
      new ProcessStartInfo("bash", "build.sh quickrun")
    else
      new ProcessStartInfo("cmd", "/c build.cmd quickrun")
  info.UseShellExecute <- false
  info.WorkingDirectory <- currentDir

  // Run the process and wait for it to complete
  let proc = Process.Start(info)
  proc.WaitForExit()
  proc.ExitCode
