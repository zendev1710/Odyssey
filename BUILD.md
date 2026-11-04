# CSMapNG

## Requirements for development

For the application development, you have to install :

- [.NET 9.0](https://dotnet.microsoft.com/fr-fr/download/dotnet/9.0) (targeted platform) - SDK and runtime
- Optionally for easier development : [Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/fr/vs/community/) IDE (VS2022), Community edition (free for personal use)

## Building the applicatoin

### Using the dotnet CLI

To build the project with the  [`dotnet CLI`](https://learn.microsoft.com/en-us/dotnet/core/tools/) (.NET 10 version) :
Using the, being in the project root directory (the one containing `Odyssey.sln`), in a command window (PowerShell, DOS...) :

```vonsole
dotnet build
```

### Using VS2022

- Double-click on the `Odyssey.sln` solution file to open it in the VS2022 IDE
- Select the wantyed active configuration (Debug or Release)
- Build solution (F6)

## Binaries

After build the executable file generated is `Odyssey.exe`.
It should be :

- In the `src/bin/Release/net8.0` and `src/bin/Release/net9.0` folders (Release configuration), or
- In the `src/bin/Debug/net8.0` and `src/bin/Debug/net9.0` folders (Debug configuration)

## Dependencies
