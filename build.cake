#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

private string SolutionFile() { return "./ObjectPool.sln"; }
private string ArtifactsDir() { return "./artifacts"; }

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
    DotNetCoreRestore();
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test-Release");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

private void Build(string cfg)
{
    DotNetCoreBuild(SolutionFile(), new DotNetCoreBuildSettings
    {
        Configuration = cfg,
        NoIncremental = true
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
    foreach (var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCorePack(project.FullPath, new DotNetCorePackSettings
        {
            Configuration = cfg,
            OutputDirectory = ArtifactsDir(),
            NoBuild = true,
            IncludeSource = true,
            IncludeSymbols = true
        });
    }
}