# Setup

To initialize oauth, run in the AnarchyChess.API project

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"

dotnet user-secrets set "Authentication:Discord:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Discord:ClientSecret" "<client-secret>"
```
