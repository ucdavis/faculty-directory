# faculty-directory

## Requirements

.NET Core 3.1 SDK (https://dotnet.microsoft.com/download/dotnet-core/3.1)

NodeJS

## Install (First Run)

In the root

`dotnet restore`

Add secrets file from 1password. Can do it manually but my trick is to type `dotnet user-secrets set a b` and it'll create you a dummy secrets file, which you can then replace with the proper version.  On a map file is located at `~/.microsoft/usersecrets/3300ed04-c997-4a05-8ce8-7a65defad5ce`

## Run

Go into the web project /src/FacultyDirectory and type `dotnet watch run`.  If you are using VSCode you can just click run or debug.

All changes to client code (styles and react) will automatically hot reload without any refresh needed.  Server side changes will automatically reload as well but take a few seconds to rebuild.

If you get an SSL error you can ignore it or add your local cert by doing `dotnet dev-certs https --trust`.  This just has to be done once per machine, and isn't a per-project thing.

### Notes

EF Migrations:
https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

`dotnet ef migrations add AddPeopleNames --project ../FacultyDirectory.Core/FacultyDirectory.Core.csproj`
