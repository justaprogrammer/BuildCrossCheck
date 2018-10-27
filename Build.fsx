
#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.Testing.XUnit2
open Fake.Core
open Fake.Tools

BuildServer.install [
    AppVeyor.Installer
]

let isAppveyor = AppVeyor.detect()
let gitVersion = GitVersion.generateProperties id

Trace.log (gitVersion.ToString())

Target.create "Clean" (fun _ ->
  !! "reports/**"
  ++ "**/SharedAssemblyInfo.cs"
  |> File.deleteAll

  let configuration = 
    (fun p -> { p with 
                  Properties = ["Configuration", "Release"]
                  Verbosity = Some MSBuildVerbosity.Minimal })

  !! "src/BCC.Core.sln"
  |> MSBuild.run configuration null "Clean" list.Empty
  |> Trace.logItems "Clean-Output: "
)

Target.create "Build" (fun _ ->
  CreateProcess.fromRawWindowsCommandLine "gitversion" "/updateassemblyinfo src\common\SharedAssemblyInfo.cs /ensureassemblyinfo"
  |> Proc.run
  |> ignore

  !! "**\*AssemblyInfo.cs"
  |> Seq.iter (fun file -> 
      Trace.log "****"
      Trace.log file

      File.read file
      |> Trace.logItems file
      Trace.log "****"
    )

  

  let configuration = (fun p -> { p with 
                                    DoRestore = true
                                    Verbosity = Some MSBuildVerbosity.Minimal })

  !! "src/BCC.Core.sln"
  |> MSBuild.runRelease configuration null "Build"
  |> Trace.logItems "AppBuild-Output: "
)

Target.create "Test" (fun _ ->
    let configuration = (fun p -> { p with
                                      HtmlOutputPath = Some "reports/tests.html"
                                      XmlOutputPath = Some "reports/tests.xml"})

    !! "src/**/bin/Release/net471/*Tests.dll"
    |> Fake.DotNet.Testing.XUnit2.run configuration
)

Target.create "Coverage" (fun _ ->
    List.allPairs ["BCC.Core.Tests" ; "BCC.Core.IntegrationTests"] ["net471" ; "netcoreapp2.1"]
    |> Seq.iter (fun (proj, framework) -> 
            let dllPath = sprintf "src\\%s\\bin\\Release\\%s\\%s.dll" proj framework proj
            let projectPath = sprintf "src\\%s\\%s.csproj" proj proj
            let reportPath = sprintf "reports/%s-%s.coverage.xml" proj framework

            sprintf "%s --target \"dotnet\" --targetargs \"test -c Release -f %s %s --no-build\" --format opencover --output \"./%s\""
                dllPath framework projectPath reportPath
            |> CreateProcess.fromRawWindowsCommandLine "coverlet"
            |> Proc.run
            |> ignore

            if isAppveyor then
                CreateProcess.fromRawWindowsCommandLine "codecov" (sprintf "-f \"%s\"" reportPath)
                |> Proc.run
                |> ignore
        )
)

Target.create "Default" (fun _ -> ())

open Fake.Core.TargetOperators
"Clean" ==> "Build"

"Build" ==> "Test"

"Build" ==> "Coverage"

"Test" ==> "Default"
"Coverage" ==> "Default"

// start build
Target.runOrDefault "Default"
