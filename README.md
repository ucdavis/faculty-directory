[![Node Version](https://img.shields.io/badge/dynamic/json?color=green&label=node&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2Ffaculty-directory%2Fmaster%2Fsrc%2FFacultyDirectory%2FClientApp%2Fpackage.json)](https://img.shields.io/badge/dynamic/json?color=green&label=node&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2Ffaculty-directory%2Fmaster%2Fsrc%2FFacultyDirectory%2FClientApp%2Fpackage.json)
[![Build Status](https://dev.azure.com/ucdavis/Faculty%20Directory/_apis/build/status%2Fucdavis.faculty-directory?branchName=master)](https://dev.azure.com/ucdavis/Faculty%20Directory/_apis/build/status%2Fucdavis.faculty-directory?branchName=master)

# faculty-directory

## Requirements

.NET 10 SDK. The SDK selection policy is in `global.json`.

NodeJS >= 22

## Install (First Run)

In the root

`dotnet restore`

Add secrets file from 1password. Can do it manually but my trick is to type `dotnet user-secrets set a b` and it'll create you a dummy secrets file, which you can then replace with the proper version.  On a map file is located at `~/.microsoft/usersecrets/3300ed04-c997-4a05-8ce8-7a65defad5ce`

## Install Tools (If you want to run migrations)

In the root

`dotnet tool restore`

Currently the only tool is dotnet-ef.

## Validate locally

`dotnet test facultydirectory.sln --configuration Release`

The backend tests exercise the application's authentication middleware, API user
management, faculty merge, and SQL Server model compatibility. They use local
fixtures and an in-memory SQLite database by default, without user secrets or
campus services.

To also exercise the existing migrations and a 2,500-person import against SQL
Server, set `FACULTY_DIRECTORY_TEST_SQL` to a connection string for a disposable
local SQL Server instance, then run the same command. Only `localhost` and
`127.0.0.1` are accepted. These tests create and delete their own uniquely named
databases and need permission to do both. Without that variable, the SQL Server
tests are reported as skipped.

Run the frontend checks from `src/FacultyDirectory/ClientApp`:

```sh
npm ci --legacy-peer-deps
npm run test:ci
npm run build
```

Azure Pipelines builds the full solution on .NET 10 and runs the backend and
frontend tests. The existing master-only packaging stage and WebJob schedules
are retained. Deploying the framework-dependent web and job packages requires
.NET 10.0.13 or later in the .NET 10 hosting runtime family. Upgrading this repository does not change
the Azure runtime settings or deploy the application.

## Run

Go into the web project /src/FacultyDirectory and type `dotnet watch run`.  If you are using VSCode you can just click run or debug.

All changes to client code (styles and react) will automatically hot reload without any refresh needed.  Server side changes will automatically reload as well but take a few seconds to rebuild.

If you get an SSL error you can ignore it or add your local cert by doing `dotnet dev-certs https --trust`.  This just has to be done once per machine, and isn't a per-project thing.

### Notes

EF Migrations:
https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

`dotnet ef migrations add AddPeopleNames --project ../FacultyDirectory.Core/FacultyDirectory.Core.csproj`

RUNNING WEB JOBS LOCALLY

Running locally via VS or VSCode debug should run in "Development" env because of launch.json

If you want to run from a command line on Mac, do `ASPNETCORE_ENVIRONMENT=Development dotnet run`
