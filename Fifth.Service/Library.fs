module Service

open Ast
open Mono.Cecil.Cil
open Mono.Cecil

open System

let instructionFor = function
    | Add -> Some OpCodes.Add
    | Number _num -> Some OpCodes.Ldc_I4
    | Output -> Some OpCodes.Call
    | _ -> None

let netStandardAssembly = 
    let userDir = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    let netStandardPath =
        System.IO.Path.Combine
            (userDir,
            ".nuget",
            "packages",
            "netstandard.library",
            "2.0.0",
            "build",
            "netstandard2.0",
            "ref",
            "netstandard.dll")
    AssemblyDefinition.ReadAssembly netStandardPath


let compile (ast: Ast.Token list) =

    let assemblyNameDefinition = AssemblyNameDefinition("TestAssembly", Version(0, 0, 1))
    let moduleKind = ModuleKind.Dll
    let assemblyDefinition = AssemblyDefinition.CreateAssembly (assemblyNameDefinition, "Program", moduleKind)
    
    let m = assemblyDefinition.MainModule

    
    let lookupNetStandardType (name: string) : TypeReference =
        netStandardAssembly.MainModule.Types
        |> Seq.find (fun t -> t.FullName = name)
        |> m.ImportReference
        
    let ourType = TypeDefinition("Program", "Bar", TypeAttributes.Public ||| TypeAttributes.Abstract ||| TypeAttributes.Sealed ||| TypeAttributes.BeforeFieldInit, (lookupNetStandardType "System.Object"))
    m.Types.Add(ourType)
    
    let method = MethodDefinition("Foo", MethodAttributes.Public ||| MethodAttributes.Static ||| MethodAttributes.HideBySig, (lookupNetStandardType "System.Void") )
    let body = MethodBody(method)

    let il = body.GetILProcessor()
    il.Append(il.Create(OpCodes.Ret))

    method.Body <- body

    ourType.Methods.Add method
    
    
    assemblyDefinition.EntryPoint <- method
    assemblyDefinition.Write "C:/Users/ridec/Desktop/TestAssembly.dll"
    ()