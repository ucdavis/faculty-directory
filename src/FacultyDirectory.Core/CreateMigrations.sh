[ "$#" -eq 1 ] || { echo "1 argument required, $# provided. Usage: sh CreateMigrationAndExecute <MigrationName>"; exit 1; }

dotnet ef migrations add $1 --context ApplicationDbContext --output-dir Migrations --startup-project ../FacultyDirectory/FacultyDirectory.csproj
dotnet ef database update --context ApplicationDbContext --startup-project ../FacultyDirectory/FacultyDirectory.csproj
# usage from PM console in the Sloth.Core directory: sh CreateMigrationAndExecute.sh <MigrationName>

echo 'All done';
