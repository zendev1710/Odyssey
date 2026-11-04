# Odyssey installer instructions

From the repository root, publish the Windows application and build the MSI:

```powershell
dotnet publish src\app\Odyssey.Desktop\Odyssey.csproj --configuration Release --framework net10.0 --runtime win-x64 --self-contained true --output installer\publish\net10.0
dotnet build installer\OdysseySetup\OdysseySetup.wixproj --configuration Release
```

The localized MSI files are written to `installer\OdysseySetup\bin\Release\<culture>\OdysseySetup.msi`.
