module Service

open Ast
open Mono.Cecil.Cil
open Mono.Cecil

open System

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


let compileFile (ast: Ast.Token list) =

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

    let imr = MethodReference("WriteLine", (lookupNetStandardType "System.Void"), (lookupNetStandardType "System.Console"))
    imr.Parameters.Add(ParameterDefinition(lookupNetStandardType "System.Int32"))
    let methodRef = m.ImportReference imr

    let opCodeFor = function
        | Add -> il.Create(OpCodes.Add)
        | Number num -> il.Create(OpCodes.Ldc_I4, num)
        | Output ->
            let imr = MethodReference("WriteLine", (lookupNetStandardType "System.Void"), (lookupNetStandardType "System.Console"))
            imr.Parameters.Add(ParameterDefinition(lookupNetStandardType "System.Int32"))
            let methodRef = m.ImportReference imr
            il.Create(OpCodes.Call, methodRef)
        | e -> failwithf "Token %A not yet supported" e

    ast |> List.iter(fun t -> il.Append(opCodeFor t))

    il.Append(il.Create(OpCodes.Ret))

    method.Body <- body

    ourType.Methods.Add method
    
    
    assemblyDefinition.EntryPoint <- method
    assemblyDefinition.Write "C:/Users/ridec/Desktop/TestAssembly.dll"
    ()

let compile (source: string) =
    let ast = Fifth.parse source
    compileFile ast