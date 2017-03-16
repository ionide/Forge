namespace <%= namespace %>

module Program =
    open System
    open System.IO
    open System.Text
    open Amazon.Lambda.Core

    let handler(context:ILambdaContext) = 
        printfn "Hello World!"