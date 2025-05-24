# Defining a Local NuGet Source and Adding a Package

If you want to upgrade to a new version and create an updated NuGet package, first increase the version number in your `.sln` file and run the following command:

```bash
dotnet build -c Release
```

If you haven't defined a local NuGet source before, add it using the command below:

```bash
dotnet nuget add source ~/.nuget/nuget-local --name Local
```

Then copy the generated NuGet package to the local source:

```bash
cp ./Navend.Core/bin/Release/*.nupkg ~/.nuget/nuget-local/
```

Now you can use the new version of the `Navend.Core` package as a local reference in your projects.

# Publish to Nuget.Org

dotnet build -c Release

dotnet nuget push bin/Release/Navend.Core.<version>.nupkg --api-key <api_key> --source https://api.nuget.org/v3/index.json