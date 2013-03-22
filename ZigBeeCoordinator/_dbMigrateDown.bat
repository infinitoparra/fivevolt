
set SchemaVersion=%1

MSBuild.exe ZigBeeCoordinator.DBMigration\ZigBeeCoordinator.DBMigration.csproj /t:MigrateDown /p:MigrationSchemaVersion=%SchemaVersion%

