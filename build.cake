#addin "nuget:?package=Cake.Wyam&version=2.1.1"
#tool "nuget:?package=Wyam&version=2.1.1"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Test");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

private string SolutionFile() { return "./object-pool.sln"; }
private string ArtifactsDir() { return "./artifacts"; }
private string NuGetFeed()    { return "https://www.myget.org/F/pomma89/api/v3/index.json"; }

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(ArtifactsDir());
});

Task("Build-Debug")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Build("Debug");
});

Task("Build-Release")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Build("Release");
});

Task("Test-Debug")
    .IsDependentOn("Build-Debug")
    .Does(() =>
{
    Test("Debug");
});

Task("Test-Release")
    .IsDependentOn("Build-Release")
    .Does(() =>
{
    Test("Release");
});

Task("Pack-Release")
    .IsDependentOn("Build-Release")
    .Does(() =>
{
    Pack("Release");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Test")
    .IsDependentOn("Test-Debug")
    .IsDependentOn("Test-Release");

Task("Docs")
    .Does(() =>
{
    Docs();
});

Task("Push")
    .IsDependentOn("Pack-Release")
    .Does(() =>
{
    Push();
});

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
    foreach (var project in GetFiles("./test/*.UnitTests/*.UnitTests.csproj"))
    {
        DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings
        {
            Configuration = cfg,
            NoBuild = true,
            NoRestore = true,
            ResultsDirectory = ArtifactsDir()
        });
    }
}

private void Pack(string cfg)
{
    foreach (var project in GetFiles("./src/**/*.csproj"))
    {
        DotNetCorePack(project.FullPath, new DotNetCorePackSettings
        {
            Configuration = cfg,
            NoBuild = true,
            NoRestore = true,
            OutputDirectory = ArtifactsDir()
        });
    }
}

private void Docs()
{
    Wyam(new WyamSettings()
    {
        InputPaths = new DirectoryPath[] { Directory("./pages") },
        OutputPath = Directory("./docs"),
        UpdatePackages = true
    });
}

private void Push()
{
    DotNetCoreNuGetPush(ArtifactsDir() + "/*.nupkg", new DotNetCoreNuGetPushSettings
    {
        Source = NuGetFeed(),
        ApiKey = EnvironmentVariable("MYGET_API_KEY")
    });
}