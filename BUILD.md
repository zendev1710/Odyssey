# CSMapNG

## Requirements for development

For the application development, you have to install :

- [.NET 10.0](https://dotnet.microsoft.com/fr-fr/download/dotnet/10.0) (targeted platform) - SDK and runtime
- Optionally for easier development : [Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/fr/vs/community/) IDE (VS2022), Community edition (free for personal use)

## Building the applicatoin

### Using the dotnet CLI

To build the solution with the [`dotnet CLI`](https://learn.microsoft.com/en-us/dotnet/core/tools/), run this command from the repository root:

```powershell
dotnet build src\Odyssey.slnx
```

### Using VS2022

- Double-click on the `Odyssey.sln` solution file to open it in the VS2022 IDE
- Select the wantyed active configuration (Debug or Release)
- Build solution (F6)

## Binaries

After build the executable file generated is `Odyssey.exe`.
It should be :

- In `src\app\Odyssey.Desktop\bin\Release\net10.0` (Release configuration), or
- In `src\app\Odyssey.Desktop\bin\Debug\net10.0` (Debug configuration)

## Testing Velopack updates locally

Release builds check GitHub Releases. For a debug build, set `ODYSSEY_UPDATE_SOURCE` to a local Velopack feed containing `releases.win.json` to test update checks without publishing a release. Install the Velopack CLI with `dotnet tool install --global vpk --version 1.2.161`.

## Dependencies
