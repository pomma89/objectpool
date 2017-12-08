#addin "nuget:?package=Cake.Wyam"
#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=Wyam"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

private string SolutionFile() { return "./ObjectPool.sln"; }
private string ArtifactsDir() { return "./artifacts"; }
private string MSBuildLinuxPath() { return @"/usr/lib/mono/msbuild/15.0/bin/MSBuild.dll"; }

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(ArtifactsDir());
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Restore();
});

Task("Build-Debug")
    .IsDependentOn("Restore")
    .Does(() => 
{
    Build("Debug");
});

Task("Build-Release")
    .IsDependentOn("Build-Debug")
    .Does(() => 
{
    Build("Release");
});

Task("Pack-Release")
    .IsDependentOn("Build-Release")
    .Does(() => 
{
    Pack("Release");
});

Task("Test-Debug")
    .IsDependentOn("Pack-Release")
    .Does(() =>
{
    Test("Debug");
});

Task("Test-Release")
    .IsDependentOn("Test-Debug")
    .Does(() =>
{
    Test("Release");
});

Task("Docs")
    .IsDependentOn("Test-Release")
    .Does(() =>
{
    Docs();
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Docs");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

private void Restore()
{
    //DotNetCoreRestore();

    MSBuild(SolutionFile(), settings =>
    {
        settings.SetMaxCpuCount(0);
        settings.SetVerbosity(Verbosity.Quiet);
        settings.WithTarget("restore");
        if (!IsRunningOnWindows())
        { 
            // Hack for Linux bug - Missing MSBuild path.
            settings.ToolPath = new FilePath(MSBuildLinuxPath());
        }
    });
}

private void Build(string cfg)
{
    //DotNetCoreBuild(SolutionFile(), new DotNetCoreBuildSettings
    //{
    //    Configuration = cfg,
    //    NoIncremental = true
    //});

    MSBuild(SolutionFile(), settings =>
    {
        settings.SetConfiguration(cfg);
        settings.SetMaxCpuCount(0);
        settings.SetVerbosity(Verbosity.Quiet);
        if (!IsRunningOnWindows())
        { 
            // Hack for Linux bug - Missing MSBuild path.
            settings.ToolPath = new FilePath(MSBuildLinuxPath());
        }
    });
}

private void Test(string cfg)
{
    //NUnit3("./test/**/bin/{cfg}/*/*.UnitTests.dll".Replace("{cfg}", cfg), new NUnit3Settings 
    //{
    //    NoResults = true
    //});

    const string flags = "--noheader --noresult --stoponerror";
    const string errMsg = " - Unit test failure - ";

    Parallel.ForEach(GetFiles("./test/*.UnitTests/**/bin/{cfg}/*/*.UnitTests.exe".Replace("{cfg}", cfg)), netExe => 
    {
        if (StartProcess(netExe, flags) != 0)
        {
            throw new Exception(cfg + errMsg + netExe);
        }
    });

    Parallel.ForEach(GetFiles("./test/*.UnitTests/**/bin/{cfg}/*/*.UnitTests.dll".Replace("{cfg}", cfg)), netCoreDll =>
    {
        DotNetCoreExecute(netCoreDll, flags);
    });
}

private void Pack(string cfg)
{
    Parallel.ForEach(GetFiles("./src/**/*.csproj"), project =>
    {
        //DotNetCorePack(project.FullPath, new DotNetCorePackSettings
        //{
        //    Configuration = cfg,
        //    OutputDirectory = ArtifactsDir(),
        //    NoBuild = true,
        //    IncludeSource = true,
        //    IncludeSymbols = true
        //});

        MSBuild(project, settings =>
        {
            settings.SetConfiguration(cfg);
            settings.SetMaxCpuCount(0);
            settings.SetVerbosity(Verbosity.Quiet);
            settings.WithTarget("pack");
            settings.WithProperty("IncludeSource", new[] { "true" });
            settings.WithProperty("IncludeSymbols", new[] { "true" });
            if (!IsRunningOnWindows())
            { 
                // Hack for Linux bug - Missing MSBuild path.
                settings.ToolPath = new FilePath(MSBuildLinuxPath());
            }
        });

        var packDir = project.GetDirectory().Combine("bin").Combine(cfg);
        MoveFiles(GetFiles(packDir + "/*.nupkg"), ArtifactsDir());
    });
}

private void Docs()
{
    if (IsRunningOnWindows())
    {
        Wyam(new WyamSettings()
        {
            InputPaths = new DirectoryPath[] { Directory("./pages") },
            OutputPath = Directory("./docs")
        });
    }
}