namespace <%= namespace %>

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI.Next
open WebSharper.UI.Next.Client

[<JavaScript>]
module Client =
    type IndexTemplate = Templating.Template<"index.html">

    let People =
        ListModel.FromSeq [
            "John"
            "Paul"
        ]

    let Main =
        JQuery.Of("#main").Empty().Ignore

        let newName = Var.Create ""

        IndexTemplate.Main.Doc(
            ListContainer = [
                People.View.DocSeqCached(fun name ->
                    IndexTemplate.ListItem.Doc(Name = View.Const name)
                )
            ],
            Name = newName,
            Add = (fun el ev ->
                People.Add(newName.Value)
                newName.Value <- ""
            )
        )
        |> Doc.RunById "main"
