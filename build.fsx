// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"
open Fake
open Fake.Testing

RestorePackages()

// Properties
let buildMode    = getBuildParamOrDefault "buildMode" "Debug"
let artifactsDir = "./.artifacts/"
let testDir      = "./Platform Specific/ObjectPool.UnitTests.*/bin/{0}/"
let testDll      = "CodeProject.ObjectPool.UnitTests.dll"
let perfDir      = "./ObjectPool.Benchmarks/bin/Release/"
let perfExe      = perfDir + "CodeProject.ObjectPool.Benchmarks.exe"
let perfResSrc   = perfDir + "BenchmarkDotNet.Artifacts/results"
let perfResDst   = artifactsDir + "perf-results"

// Common - Build
let myBuild target buildMode =
    let setParams defaults = 
      { defaults with
          Verbosity = Some(Quiet)
          Targets = [target]
          Properties = 
            [
              "Configuration", buildMode
            ]
      }
    build setParams "./ObjectPool.sln" |> DoNothing

// Common - Test
let myTest (buildMode: string) =
    !! (System.String.Format(testDir, buildMode) + testDll)
      |> NUnit3 (fun p -> 
        { p with
            ShadowCopy = true;
            OutputDir = artifactsDir + "test-results.xml"
            ResultSpecs  = [ "test-results.xml" ]
        })

// Targets
Target "Clean" (fun _ ->
    trace "Cleaning..."
    
    CleanDirs [artifactsDir]

    myBuild "Clean" "Debug"
    myBuild "Clean" "Release"
    myBuild "Clean" "Publish"
)

Target "BuildDebug" (fun _ ->
    trace "Building for DEBUG..."
    myBuild "Build" "Debug"
)

Target "BuildRelease" (fun _ ->
    trace "Building for RELEASE..."
    myBuild "Build" "Release"
)

Target "BuildPublish" (fun _ ->
    trace "Building for PUBLISH..."
    myBuild "Build" "Publish"
)

Target "TestDebug" (fun _ ->
    trace "Testing for DEBUG..."
    myTest "Debug"
)

Target "TestRelease" (fun _ ->
    trace "Testing for RELEASE..."
    myTest "Release"
)

Target "PerfRelease" (fun _ ->
    trace "Testing performance..."
    let ok = directExec (fun info ->
      info.FileName <- perfExe
      info.WorkingDirectory <- perfDir)
    if ok then CopyDir perfResDst perfResSrc (fun s -> true)
)

Target "Default" (fun _ ->
    trace "Building and publishing Thrower..."
)

// Dependencies
"Clean"
  ==> "BuildDebug"
  ==> "TestDebug"
  ==> "BuildRelease"
  ==> "TestRelease"
  ==> "PerfRelease"
  ==> "BuildPublish"
  ==> "Default"

// Start build
RunTargetOrDefault "Default"