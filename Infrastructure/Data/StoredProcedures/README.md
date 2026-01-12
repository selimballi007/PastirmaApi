# PostgreSQL Stored Procedures

This folder contains the PostgreSQL stored procedures (functions) used by the PastirmaApi application.

## Overview

These stored procedures are automatically applied to the database through Entity Framework Core migrations. They are used to optimize database operations and ensure atomic updates with read operations.

## Stored Procedures

### 1. `update_refresh_token`

**Purpose**: Updates a user's refresh token while validating the old token and returns the last login timestamp.

**Parameters**:
- `p_user_id` (INTEGER): The user's ID
- `p_old_refresh_token` (TEXT): Current refresh token for validation
- `p_new_refresh_token` (TEXT): New refresh token to set
- `p_new_refresh_token_expiry` (TIMESTAMP WITH TIME ZONE): Expiry timestamp for the new token

**Returns**: Table with `last_login_at` (TIMESTAMP WITH TIME ZONE)

**Usage in Code**:
```csharp
// UserRepository.cs - UpdateTokenAsync method
var result = await _context.Database
    .SqlQueryRaw<RefreshTokenDTO>(
        "SELECT * FROM update_refresh_token(@p0, @p1, @p2, @p3)",
        dto.Id, dto.oldRefreshToken, dto.newRefreshToken, dto.newRefreshTokenExpiry
    )
    .FirstOrDefaultAsync();
```

**Security Features**:
- Validates the old refresh token before updating
- Only updates active users (is_active = true)
- Updates the updated_at timestamp automatically

---

### 2. `get_and_update_user_login`

**Purpose**: Retrieves user information by email and atomically updates login-related fields (last_login_at, refresh_token).

**Parameters**:
- `p_email` (TEXT): User's email address
- `p_refresh_token` (TEXT): New refresh token to set
- `p_refresh_token_expiry` (TIMESTAMP WITH TIME ZONE): Expiry timestamp for the refresh token

**Returns**: Table with complete user information:
- `id` (INTEGER)
- `username` (TEXT)
- `email` (TEXT)
- `role` (TEXT)
- `password_hash` (TEXT)
- `last_login_at` (TIMESTAMP WITH TIME ZONE)
- `refresh_token` (TEXT)
- `refresh_token_expiry` (TIMESTAMP WITH TIME ZONE)
- `is_verified` (BOOLEAN)

**Usage in Code**:
```csharp
// UserRepository.cs - GetAndUpdateLoginAsync method
var result = await _context.Database
    .SqlQueryRaw<LoginUserRawDTO>(
        "SELECT * FROM public.get_and_update_user_login({0}, {1}, {2})",
        email, refreshToken, refreshTokenExpiry)
    .AsNoTracking()
    .FirstOrDefaultAsync();
```

**Security Features**:
- Only returns active users (is_active = true)
- Atomically updates login timestamp with token refresh
- Updates the updated_at timestamp automatically

---

## Deployment

### Applying to Database

The stored procedures are applied through Entity Framework Core migrations:

```bash
# Apply all pending migrations including stored procedures
dotnet ef database update

# Or if using the specific migration
dotnet ef database update AddStoredProcedures
```

### Manual Execution

If you need to manually create these functions in your PostgreSQL database, execute the SQL files in this folder:

1. `update_refresh_token.sql`
2. `get_and_update_user_login.sql`

```bash
psql -U your_user -d your_database -f Infrastructure/Data/StoredProcedures/update_refresh_token.sql
psql -U your_user -d your_database -f Infrastructure/Data/StoredProcedures/get_and_update_user_login.sql
```

---

## Migration Information

These stored procedures are managed by the following migration:
- **Migration Name**: `AddStoredProcedures`
- **Migration File**: `Infrastructure/Data/Migrations/*_AddStoredProcedures.cs`
- **Created**: 2025-12-26

### Rolling Back

To remove the stored procedures from the database:

```bash
# Revert to the migration before AddStoredProcedures
dotnet ef database update <previous_migration_name>
```

Or manually drop them:

```sql
DROP FUNCTION IF EXISTS public.update_refresh_token(INTEGER, TEXT, TEXT, TIMESTAMP WITH TIME ZONE);
DROP FUNCTION IF EXISTS public.get_and_update_user_login(TEXT, TEXT, TIMESTAMP WITH TIME ZONE);
```

---

## Best Practices

1. **Always use parameterized queries** when calling these stored procedures from C# to prevent SQL injection
2. **Test stored procedure changes** in a development environment before deploying to production
3. **Version control** all SQL files in this folder
4. **Update this README** when adding new stored procedures or modifying existing ones
5. **Use migrations** to deploy stored procedure changes rather than manual SQL execution

---

## Troubleshooting

### Function Not Found Error

If you get "function does not exist" error:
1. Check if migrations have been applied: `dotnet ef migrations list`
2. Apply pending migrations: `dotnet ef database update`
3. Verify in PostgreSQL: `\df public.update_refresh_token`

### Permission Issues

If you get permission errors when calling the functions:
```sql
GRANT EXECUTE ON FUNCTION public.update_refresh_token(INTEGER, TEXT, TEXT, TIMESTAMP WITH TIME ZONE) TO your_app_role;
GRANT EXECUTE ON FUNCTION public.get_and_update_user_login(TEXT, TEXT, TIMESTAMP WITH TIME ZONE) TO your_app_role;
```

---

## Related Files

- **Repository**: `Infrastructure/Data/Repositories/UserRepository.cs`
- **DTOs**: `Application/DTOs/UserDTOs/` (RefreshTokenDTO, LoginUserRawDTO, etc.)
- **Migration**: `Infrastructure/Data/Migrations/*_AddStoredProcedures.cs`
