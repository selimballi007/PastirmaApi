# Secrets Management Guide

This project uses **User Secrets** for development and **Environment Variables** for production to keep sensitive data secure.

## Quick Start (Development)

### 1. Required Secrets

All sensitive configuration is stored in User Secrets. Use the following commands to set them up:

```bash
# Database Connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=your-host;Database=your-db;Username=user;Password=pass"

# JWT Configuration
dotnet user-secrets set "Jwt:Key" "your-32-character-secret-key-minimum"
dotnet user-secrets set "Jwt:Issuer" "PastirmaApi"
dotnet user-secrets set "Jwt:Audience" "PastirmaClient"
dotnet user-secrets set "Jwt:AccessTokenExpiresMinutes" "15"
dotnet user-secrets set "Jwt:RefreshTokenExpiresDays" "7"
dotnet user-secrets set "Jwt:EmailTokenExpiresDays" "1"
dotnet user-secrets set "Jwt:PasswordResetTokenExpiresMinutes" "60"

# Cloudinary (Image Storage)
dotnet user-secrets set "Cloudinary:CloudName" "your-cloud-name"
dotnet user-secrets set "Cloudinary:ApiKey" "your-api-key"
dotnet user-secrets set "Cloudinary:ApiSecret" "your-api-secret"

# Resend (Email Service)
dotnet user-secrets set "Resend:RESEND_API_KEY" "re_your_api_key"
dotnet user-secrets set "Resend:EmailFrom" "noreply@yourdomain.com"
dotnet user-secrets set "Resend:AdminEmail" "admin@yourdomain.com"

# Cloudflare Turnstile (CAPTCHA)
dotnet user-secrets set "Turnstile:SecretKey" "your-turnstile-secret"

# CORS Origins (Frontend URLs)
dotnet user-secrets set "CorsSettings:AllowedOrigins:0" "http://localhost:3000"
dotnet user-secrets set "CorsSettings:AllowedOrigins:1" "http://192.168.1.104:3000"

# Frontend URL (for email links)
dotnet user-secrets set "FrontendUrl" "http://localhost:3000"

# Allowed Hosts (for host header validation)
dotnet user-secrets set "AllowedHosts" "*"
```

### 2. View Current Secrets

```bash
dotnet user-secrets list
```

### 3. Remove a Secret

```bash
dotnet user-secrets remove "SecretKey"
```

### 4. Clear All Secrets

```bash
dotnet user-secrets clear
```

## Configuration Files Strategy

### ✅ Committed to Git (appsettings.json)
- Logging configuration only

**These files contain NO sensitive data and are safe to commit.**

### ❌ NOT Committed (User Secrets / Environment Variables)
- **ALL application configuration** is now in user secrets/environment variables
- Database passwords
- API keys and secrets
- JWT settings (Key, Issuer, Audience, expiration times)
- Email service credentials
- CORS origins (environment-specific)
- AllowedHosts (environment-specific)
- Frontend URL

**Benefit**: Nothing forgotten during deployment - everything must be explicitly configured!

## Production Deployment

For production environments (Railway, Azure, AWS, etc.), use **Environment Variables** instead of user secrets.

### Railway

In Railway Dashboard → Variables:

```bash
ConnectionStrings__DefaultConnection=Host=prod-host;Database=prod-db;...
Jwt__Key=your-production-secret-key
Jwt__Issuer=PastirmaApi
Jwt__Audience=PastirmaClient
Jwt__AccessTokenExpiresMinutes=15
Jwt__RefreshTokenExpiresDays=7
Jwt__EmailTokenExpiresDays=1
Jwt__PasswordResetTokenExpiresMinutes=60
Cloudinary__CloudName=your-cloud-name
Cloudinary__ApiKey=your-api-key
Cloudinary__ApiSecret=your-api-secret
Resend__RESEND_API_KEY=re_your_api_key
Resend__EmailFrom=noreply@yourdomain.com
Resend__AdminEmail=admin@yourdomain.com
Turnstile__SecretKey=your-turnstile-secret
CorsSettings__AllowedOrigins__0=https://yourdomain.com
CorsSettings__AllowedOrigins__1=https://www.yourdomain.com
FrontendUrl=https://yourdomain.com
AllowedHosts=yourdomain.com;www.yourdomain.com
```

**Note:** Use double underscores `__` for nested configuration in environment variables.

### Azure App Service

In Azure Portal → Configuration → Application Settings:

```
ConnectionStrings__DefaultConnection = Host=...
Jwt__Key = your-key
(same pattern as Railway)
```

### Docker / docker-compose.yml

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=...
  - Jwt__Key=your-key
  - CorsSettings__AllowedOrigins__0=https://yourdomain.com
```

## Security Best Practices

1. ✅ **Never commit secrets to git**
2. ✅ **Use different secrets for dev/staging/production**
3. ✅ **Rotate secrets regularly**
4. ✅ **Use strong, randomly generated keys for JWT**
5. ✅ **Keep user secrets backed up securely**
6. ✅ **Review secrets before deploying**

## Troubleshooting

### "Configuration is not configured" Error

This means a required secret is missing. Check:

1. Run `dotnet user-secrets list` to see current secrets
2. Compare with `appsettings.Template.json` to see required structure
3. Add missing secrets using `dotnet user-secrets set`

### Secrets Not Loading

User secrets are only loaded in **Development** environment. In production:
- Use environment variables instead
- Ensure `ASPNETCORE_ENVIRONMENT` is set correctly

## Template File

See `appsettings.Template.json` for the complete structure of required configuration (without actual values).

## Where Secrets Are Stored

**Development (User Secrets):**
- Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

**Production:**
- Environment variables provided by hosting platform
- Never stored in files in production

## For New Team Members

1. Clone the repository
2. Review `appsettings.Template.json` to see required secrets
3. Get actual secret values from team lead (via secure channel)
4. Run the setup commands from "Required Secrets" section above
5. Verify with `dotnet user-secrets list`
6. Run `dotnet build` to confirm configuration is valid
