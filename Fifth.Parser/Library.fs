module Fifth

open FSharp.Text.Lexing

let parse (input: string) = 
    let lexbuf = LexBuffer<char>.FromString input
    let res = Parser.start Lexer.tokenstream  lexbuf
    res