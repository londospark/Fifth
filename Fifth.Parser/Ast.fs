module Ast

type Token =
    | Add
    | Subtract
    | Multiply
    | Divide
    | Output
    | Number of int