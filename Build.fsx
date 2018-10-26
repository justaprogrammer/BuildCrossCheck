
#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.Testing.XUnit2
open Fake.Core

Target.create "Clean" (fun _ ->
  !! "BCC.Core.sln"
    |> MSBuild.run 
        (fun p -> { p with Properties = ["Configuration", "Release"] })
        null
        "Clean"
        list.Empty
    |> Trace.logItems "Clean-Output: "
)

Target.create "Build" (fun _ ->
  !! "BCC.Core.sln"
    |> MSBuild.run id null "Restore" list.Empty
    |> Trace.logItems "Restore-Output: "

  !! "BCC.Core.sln"
    |> MSBuild.runRelease id null "Build"
    |> Trace.logItems "AppBuild-Output: "
)

Target.create "Test" (fun _ ->
    !! "**/bin/Release/net471/*Tests.dll"
    |> Fake.DotNet.Testing.XUnit2.run (fun p -> { p with HtmlOutputPath = Some "reports/xunit.html" })
)

Target.create "Default" (fun _ -> ())

open Fake.Core.TargetOperators
"Clean" ==> "Build" ==> "Test" ==> "Default"

// start build
Target.runOrDefault "Default"
