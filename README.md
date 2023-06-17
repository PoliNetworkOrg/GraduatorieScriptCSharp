# GraduatorieScriptCSharp

Rewrite in C# della repo https://github.com/PoliNetworkOrg/GraduatorieScript

## Requirements
- [dotnet sdk >= 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [git](https://git-scm.com/downloads)

### Ubuntu
> Note: this may upgrade older versions installed 
```sh
sudo apt-get update && sudo apt-get install dotnet-sdk-7.0 git
```

## Quickstart
1. get git submodules (libraries)
```sh
git submodule update --init --recursive
```
2. install dotnet deps
```sh
dotnet restore
```
3. run
```sh
dotnet run
```
