#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.JavaScript
open Fake.Core
open Fake.Tools

BuildServer.install [
    AppVeyor.Installer
]

let isAppveyor = AppVeyor.detect()
let gitVersion = GitVersion.generateProperties id

Target.create "Clean" (fun _ ->
  ["build" ; "reports" ; "src/common" ; "src/BCC.Web/node_modules"]
  |> Shell.cleanDirs

  let configuration = 
    (fun p -> { p with 
                  Properties = ["Configuration", "Release"]
                  Verbosity = Some MSBuildVerbosity.Minimal })

  !! "src/BCC.Web.sln"
  |> MSBuild.run configuration null "Clean" list.Empty
  |> Trace.logItems "Clean-Output: "
)

Target.create "Build" (fun _ ->
  CreateProcess.fromRawWindowsCommandLine "gitversion" "/updateassemblyinfo src\common\SharedAssemblyInfo.cs /ensureassemblyinfo"
  |> Proc.run
  |> ignore

  Npm.install (fun p -> {p with WorkingDirectory = "src/BCC.Web"})

  let configuration = (fun p -> { p with 
                                    DoRestore = true
                                    Verbosity = Some MSBuildVerbosity.Minimal })

  !! "src/BCC.Web.sln"
  |> MSBuild.runRelease configuration null "Build"
  |> Trace.logItems "AppBuild-Output: "
)

Target.create "Test" (fun _ ->
    DotNet.test (fun t -> {t with
                             Configuration = DotNet.BuildConfiguration.Release
                             Logger = Some "trx;LogFileName=results.trx"
                             ResultsDirectory = Some "../../reports"})
                "src/BCC.Web.Tests/BCC.Web.Tests.csproj"

    Trace.publish ImportData.BuildArtifact "reports/results.trx"
)

Target.create "Package" (fun _ ->
    DotNet.publish (fun p -> {p with 
                                Configuration = DotNet.BuildConfiguration.Release
                                OutputPath = Some "../../build"}) "src/BCC.Web"
)

Target.create "Coverage" (fun _ ->
    List.allPairs ["BCC.Web.Tests"] ["netcoreapp2.1"]
    |> Seq.iter (fun (proj, framework) -> 
            let dllPath = sprintf "src\\%s\\bin\\Release\\%s\\%s.dll" proj framework proj
            let projectPath = sprintf "src\\%s\\%s.csproj" proj proj
            let reportPath = sprintf "reports/%s-%s.coverage.xml" proj framework

            sprintf "%s --target \"dotnet\" --targetargs \"test -c Release -f %s %s --no-build\" --format opencover --output \"./%s\""
                dllPath framework projectPath reportPath
            |> CreateProcess.fromRawWindowsCommandLine "coverlet"
            |> Proc.run
            |> ignore

            Trace.publish ImportData.BuildArtifact reportPath

            if isAppveyor then
                CreateProcess.fromRawWindowsCommandLine "codecov" (sprintf "-f \"%s\"" reportPath)
                |> Proc.run
                |> ignore
        )
)

Target.create "Default" (fun _ -> ())

open Fake.Core.TargetOperators
"Clean" ==> "Build"

"Build" ==> "Package" ==> "Default"
"Build" ==> "Test" ==> "Default"
"Build" ==> "Coverage" ==> "Default"

// start build
Target.runOrDefault "Default"
