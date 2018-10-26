
#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.Testing.XUnit2
open Fake.Core

BuildServer.install [
    AppVeyor.Installer
]

let isAppveyor = AppVeyor.detect()

Target.create "Clean" (fun _ ->
  !! "reports/**"
  |> File.deleteAll

  let configuration = 
    (fun p -> { p with 
                  Properties = ["Configuration", "Release"]
                  DisableInternalBinLog=true })

  !! "src/BCC.Core.sln"
  |> MSBuild.run configuration null "Clean" list.Empty
  |> Trace.logItems "Clean-Output: "
)

Target.create "Build" (fun _ ->
  let configuration = (fun p -> { p with DoRestore = true })

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

// coverlet .\BCC.Core.Tests\bin\Release\net471\BCC.Core.Tests.dll 
// --target "dotnet"
// --targetargs \"test -c Release .\BCC.Core.Tests\BCC.Core.Tests.csproj --no-build\"
// --format opencover
// --output ".\BCC.Core.Tests-net471.coverage.xml"

Target.create "Coverage" (fun _ ->
    List.allPairs ["BCC.Core.Tests" ; "BCC.Core.IntegrationTests"] ["net471" ; "netcoreapp2.1"]
    |> Seq.iter (fun (proj, framework) -> 
            [
                (sprintf "src\\%s\\bin\\Release\\%s\\%s.dll" proj framework proj) ;
                "--target \"dotnet\"" ;
                (sprintf "--targetargs \"test -c Release -f %s src\\%s\\%s.csproj --no-build\"" framework proj proj) ;
                "--format opencover" ;
                (sprintf "--output \"./reports/%s-%s.coverage.xml\"" proj framework) ;
            ]
            |> String.concat " "
            |> CreateProcess.fromRawWindowsCommandLine "coverlet"
            |> Proc.run
            |> ignore
        )
)

Target.create "Default" (fun _ -> ())

let hasCoverlet = true

open Fake.Core.TargetOperators
"Clean" ==> "Build"
"Build" ==> "Test"
"Build" =?> ("Coverage", hasCoverlet)
"Test" ==> "Default"
"Coverage" ==> "Default"

// start build
Target.runOrDefault "Default"
