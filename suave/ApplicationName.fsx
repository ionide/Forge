#r "<%= packagesPath %>/Suave/lib/net40/Suave.dll"

open Suave                 // always open suave
open Suave.Successful      // for OK-result
open Suave.Web             // for config

startWebServer defaultConfig (OK "Hello World!")
