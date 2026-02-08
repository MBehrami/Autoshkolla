# Create Candidates Tables

The error **"Invalid object name 'Candidates'."** means the `Candidates` table (and related tables) do not exist in your database.

## Run the script

Use the same server, database, and login as in `appsettings.json` (e.g. `ApiConnStringMssql`).

### Option 1: Terminal (sqlcmd)

From this folder (`AdminApi/Scripts`):

```bash
sqlcmd -S localhost,1433 -d AutoshkollaDb -U sa -P 'Str0ng!Passw0rd' -i CreateCandidatesTables.sql -C
```

- `-C` trusts the server certificate (typical for local SQL Server).
- If your DB name, user, or password differ, change `-d`, `-U`, `-P` to match `appsettings.json`.

### Option 2: SSMS / Azure Data Studio

1. Connect to `localhost,1433` with the same user/password as in your connection string.
2. Open `CreateCandidatesTables.sql`.
3. Ensure you are connected to database **AutoshkollaDb** (or change the `USE [AutoshkollaDb]` line).
4. Execute the script.

### After the script runs

Restart the Admin API and reload the Candidates page. The list should load without the "Invalid object name 'Candidates'." error.
