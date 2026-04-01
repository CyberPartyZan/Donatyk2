To add migrations to the project, run the following command in the Package Manager Console:
	dotnet ef migrations add <MigrationName> --project Marketplace.Repository.MSSql/Marketplace.Repository.MSSql.csproj --startup-project Marketplace.Server/Marketplace.Server.csproj
	dotnet ef migrations remove --project Marketplace.Repository.MSSql/Marketplace.Repository.MSSql.csproj --startup-project Marketplace.Server/Marketplace.Server.csproj
	
To revert the database to a previous migration, run the following command in the Package Manager Console:
	dotnet ef database update <MigrationName> --project Marketplace.Repository.MSSql/Marketplace.Repository.MSSql.csproj --startup-project Marketplace.Server/Marketplace.Server.csproj

To update the database with the latest migrations, run the following command in the Package Manager Console:
	dotnet ef database update --project Marketplace.Repository.MSSql/Marketplace.Repository.MSSql.csproj --startup-project Marketplace.Server/Marketplace.Server.csproj