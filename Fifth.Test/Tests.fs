module Tests

open Xunit

open Fifth
open Ast
open Service

open FsUnit.Xunit


[<Fact>]
let ``Simple Calculation`` () =
    parse "31 4 +" |> should equal [ Number 31; Number 4; Add ]

[<Fact>]
let ``Create DLL`` () =
    compile "6 7 + ." |> ignore