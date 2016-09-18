namespace <%= namespace %>
    module App=
    
    open Nancy

    type App() as this =
        inherit NancyModule()
        do
            this.Get.["/"] <- fun _ -> "Hello World!" :> obj