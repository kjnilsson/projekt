module Projekt.Main
open System
open System.IO
open System.Xml.Linq

[<EntryPoint>]
let main argv =
    let op = 
        match Args.parse argv with
        | Success op -> op 
        | Failure msg ->
            eprintfn "%s" msg
            Exit

    let save (el : XElement) (path: string) =
        try
            el.Save path
            0
        with
        | ex ->
            eprintfn "err: failed to save %s. Message: %s" path ex.Message
            1

    let saveOrPrintError path (result: Result<XElement>) : int =
        match result with
        | Success el -> save el path
        | Failure msg -> eprintfn "%s" msg; 1

    let templatesDir = 
        //slight hack to allow the templates directory to be located 
        //in the same directory as the exe for distribution 
        let cur = Reflection.Assembly.GetExecutingAssembly().CodeBase </> "templates"
        if Directory.Exists cur then cur
        else "templates" 
    
    match op with
    | Init data ->
        match Template.init templatesDir data with
        | Success _ -> 0
        | Failure msg ->
            eprintfn "%s" msg
            1
    | AddFile data ->
        Project.addFile data.ProjPath data.FilePath data.Link data.Compile
        |> saveOrPrintError data.ProjPath

    | DelFile data ->
        (Project.delFile data.ProjPath data.FilePath)
        |> saveOrPrintError data.ProjPath

    | MoveFile data ->
        (Project.moveFile data.ProjPath data.FilePath data.Direction data.Repeat)
        |> saveOrPrintError data.ProjPath
        
    | Reference { ProjPath = path; Reference = reference } ->
        Project.addReference path reference
        |> saveOrPrintError path 
    | _ -> 
        1

