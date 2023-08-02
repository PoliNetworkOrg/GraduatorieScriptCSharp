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
dotnet run --project PoliNetwork.Graduatorie.Parser
```

Projects:
- `PoliNetwork.Graduatorie.Scraper` scrapes links from PoliMi and obtain html
- `PoliNetwork.Graduatorie.Parser` runs scraper then parses html into objects/json

Parameters:
- `--reparse` regenerate all rankings files (use it when make changes to data structure)
- `--data dir` specify where is the data folder

### Data folder

Data folder has been moved to [RankingsDati](https://github.com/PoliNetworkOrg/RankingsDati)
in [#128](https://github.com/PoliNetworkOrg/GraduatorieScriptCSharp/issues/128)
therefore the script won't find it.

**Clone this repo and RankingsDati** then follow one of these two methods:

- Run the script specifying where is the data folder with the `--data dir` param.

   ```sh
    dotnet run --project PoliNetwork.Graduatorie.Parser --data ../RankingsDati/data
   ```

- Create a symbolic link inside the script repo pointing to the data folder inside RankingsDati
   so that the script would find it independently.

   ```sh
    ln -s ../RankingsDati/data/
    dotnet run --project PoliNetwork.Graduatorie.Parser
   ```

   Git should ignore the link created (because `data` is inside `.gitignore`)

The script output will be in the data folder found (copied on reference).  
**In both cases you may check that RankingsDati is up-to-date to avoid
false positive or incomplete output.**
