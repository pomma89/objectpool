#tool nuget:?package=NUnit.ConsoleRunner&version=3.6.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionFile = "./ObjectPool.sln";
var artifactsDir = "./artifacts";
var testResultsDir = artifactsDir + "/test-results";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectory(testResultsDir);
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

Task("Test-Debug")
    .IsDependentOn("Build-Debug")
    .Does(() =>
{
    Test("Debug");
});

Task("Build-Release")
    .IsDependentOn("Test-Debug")
    .Does(() => 
{
    Build("Release");
});

Task("Test-Release")
    .IsDependentOn("Build-Release")
    .Does(() =>
{
    Test("Release");
});

Task("Pack-Release")
    .IsDependentOn("Test-Release")
    .Does(() => 
{
    Pack("Release");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack-Release");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

private void Build(string cfg)
{
    foreach(var project in GetFiles("./**/project.json"))
    {
        DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings
        {
            Configuration = cfg,
            NoIncremental = true
        });
    }
}

private void Test(string cfg)
{
    NUnit3("./test/**/bin/{cfg}/*/*.UnitTests.dll".Replace("{cfg}", cfg), new NUnit3Settings 
    {
        NoResults = true,
        OutputFile = testResultsDir + "/" + cfg.ToLower() + ".xml"
    });
}

private void Pack(string cfg)
{
    foreach (var project in GetFiles("./src/**/project.json"))
    {
        DotNetCorePack(project.FullPath, new DotNetCorePackSettings
        {
            Configuration = cfg,
            OutputDirectory = artifactsDir,
            NoBuild = true
        });
    }    
}