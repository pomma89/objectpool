#tool nuget:?package=NUnit.ConsoleRunner&version=3.6.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionFile = "./ObjectPool.sln";
var artifactsDir = "./artifacts";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
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
    //foreach(var project in GetFiles("./**/*.csproj"))
    //{
    //    DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings
    //    {
    //        Configuration = cfg,
    //        NoIncremental = true
    //    });
    //}
    
    MSBuild(solutionFile, settings =>
    {
        settings.SetConfiguration(cfg);
        settings.SetMaxCpuCount(0);
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
        //    OutputDirectory = artifactsDir,
        //    NoBuild = true
        //});

        MSBuild(project, settings =>
        {
            settings.SetConfiguration(cfg);
            settings.SetMaxCpuCount(0);
            settings.WithTarget("pack");
            settings.WithProperty("IncludeSymbols", new[] { "true" });
        });

        var packDir = project.GetDirectory().Combine("bin").Combine(cfg);
        MoveFiles(GetFiles(packDir + "/*.nupkg"), artifactsDir);
    });
}