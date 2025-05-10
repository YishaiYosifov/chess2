# Setup

To initialize google oauth, run in the Chess2.API project

```bash
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"
```
