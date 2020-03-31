// Learn more about F# at http://fsharp.org

open System
open Ooui


[<EntryPoint>]
let main argv =

    let button = Button("Click Me!")

    let mutable count = 0
    button.Click.Add(fun _ ->
        count <- count + 1
        button.Text <- (sprintf "Clicked %d times." count))
    
    // Required to run as a non-admin: https://github.com/praeclarum/Ooui/issues/152#issuecomment-419639448
    UI.Host <- "localhost"
    UI.Publish ("/", button)
    UI.Present ("/")

    Console.ReadLine () |> ignore
    0 // return an integer exit code
