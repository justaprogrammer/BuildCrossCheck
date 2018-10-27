#r "paket: groupref FakeBuild //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.BuildServer
open Fake.IO.Globbing.Operators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.DotNet.Testing.XUnit2
open Fake.Core
open Fake.Tools
open Fake.IO

BuildServer.install [
    AppVeyor.Installer
]

let isAppveyor = AppVeyor.detect()
let gitVersion = GitVersion.generateProperties id

Target.create "Clean" (fun _ ->
  ["reports" ; "nuget" ; "src/common"]
  |> Shell.cleanDirs

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

  let configuration = (fun p -> { p with 
                                    DoRestore = true
                                    Verbosity = Some MSBuildVerbosity.Minimal })

  !! "src/BCC.Core.sln"
  |> MSBuild.runRelease configuration null "Build"
  |> Trace.logItems "AppBuild-Output: "
)

Target.create "Test" (fun _ ->
    let testHtmlReport = "reports/tests.html"
    let testXmlReport = "reports/tests.html"

    let configuration = (fun p -> { p with
                                      HtmlOutputPath = Some testHtmlReport
                                      XmlOutputPath = Some testXmlReport})

    !! "src/**/bin/Release/net471/*Tests.dll"
    |> Fake.DotNet.Testing.XUnit2.run configuration

    Trace.publish ImportData.BuildArtifact testHtmlReport
    Trace.publish ImportData.BuildArtifact testXmlReport
)

Target.create "Package" (fun _ ->
    Shell.mkdir "nuget"
    
    !! "Package.nuspec"
    |> Shell.copy "nuget"

    Shell.copyRecursive "src/BCC.Core/bin/Release" "nuget/lib" false
    |> ignore

    let version = 
        match String.isNullOrWhiteSpace gitVersion.PreReleaseLabel with
        | false -> sprintf "%s-beta-%s%s" gitVersion.MajorMinorPatch gitVersion.PreReleaseLabel gitVersion.BuildMetaDataPadded
        | _ -> sprintf "%s-beta" gitVersion.MajorMinorPatch

    NuGet.NuGetPack (fun p -> { p with
                                  Version = version
                                  OutputPath = "nuget" }) "nuget/Package.nuspec"

    !! "nuget/*.nupkg"
    |> Seq.iter (Trace.publish ImportData.BuildArtifact)
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
