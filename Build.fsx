
#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.Testing.XUnit2
open Fake.Core

Target.create "Clean" (fun _ ->
  let configuration = 
    (fun p -> { p with 
                  Properties = ["Configuration", "Release"]
                  DisableInternalBinLog=true })

  !! "BCC.Core.sln"
  |> MSBuild.run configuration null "Clean" list.Empty
  |> Trace.logItems "Clean-Output: "
)

Target.create "Build" (fun _ ->
  let configuration = (fun p -> { p with DoRestore = true })

  !! "BCC.Core.sln"
  |> MSBuild.runRelease configuration null "Build"
  |> Trace.logItems "AppBuild-Output: "
)

Target.create "Test" (fun _ ->
    let configuration = (fun p -> { p with HtmlOutputPath = Some "reports/xunit.html" })

    !! "**/bin/Release/net471/*Tests.dll"
    |> Fake.DotNet.Testing.XUnit2.run configuration
)

Target.create "Default" (fun _ -> ())

open Fake.Core.TargetOperators
"Clean" ==> "Build" ==> "Test" ==> "Default"

// start build
Target.runOrDefault "Default"
