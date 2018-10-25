#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.Testing.XUnit2
nuget Fake.Core.Target
nuget xunit.runner.console 2.4.0 //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.Testing.XUnit2
open Fake.Core

Target.create "Clean" (fun _ ->
  !! "BCC.Core.sln"
    |> MSBuild.run id null "Clean" list.Empty
    |> Trace.logItems "AppBuild-Output: "
)

Target.create "Build" (fun _ ->
  !! "BCC.Core.sln"
    |> MSBuild.runRelease id null "Build"
    |> Trace.logItems "AppBuild-Output: "
)

Target.create "Test" (fun _ ->
    !! "**/*Tests.dll"
    |> Fake.DotNet.Testing.XUnit2.run (fun p -> { p with
                                                    HtmlOutputPath = Some "xunit.html"
                                                    ToolPath = System.Environment.ExpandEnvironmentVariables("%HOME%/.nuget/packages/xunit.runner.console/2.4.0/tools/net452/xunit.console.exe")
                                                    })
)

Target.create "Default" (fun _ -> ())

open Fake.Core.TargetOperators

"Clean" ==> "Build" ==> "Test" ==> "Default"

// start build
Target.runOrDefault "Default"
