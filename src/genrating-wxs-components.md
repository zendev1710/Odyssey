# how-to

## generae a wws

In the src folder :

- Open a Windows PowerShell instance as administrator
- > Get-ExecutionPolicy => restricted
- > Set-ExecutionPolicy RemoteSigned
- > .\generate-wix-components.ps1 -PublishDir "publish\net8.0" -OutputFile "setup\ComponentsForNet80.wxs"
- > .\generate-wix-components.ps1 -PublishDir "publish\net9.0" -OutputFile "setup\ComponentsForNet90.wxs"
