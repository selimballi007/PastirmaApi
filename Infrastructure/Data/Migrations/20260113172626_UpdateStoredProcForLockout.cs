using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastirmaApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoredProcForLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update get_and_update_user_login stored procedure to include lockout fields
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION public.get_and_update_user_login(
                    p_email TEXT,
                    p_refresh_token TEXT,
                    p_refresh_token_expiry TIMESTAMP WITH TIME ZONE
                )
                RETURNS TABLE (
                    id INTEGER,
                    username TEXT,
                    email TEXT,
                    role INTEGER,
                    password_hash TEXT,
                    last_login_at TIMESTAMP WITH TIME ZONE,
                    refresh_token TEXT,
                    refresh_token_expiry TIMESTAMP WITH TIME ZONE,
                    is_verified BOOLEAN,
                    failed_login_attempts INTEGER,
                    lockout_end TIMESTAMP WITH TIME ZONE
                )
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    -- Update user's last login timestamp and refresh token
                    UPDATE users
                    SET
                        last_login_at = CURRENT_TIMESTAMP,
                        refresh_token = p_refresh_token,
                        refresh_token_expiry = p_refresh_token_expiry,
                        updated_date = CURRENT_TIMESTAMP
                    WHERE
                        users.email = p_email
                        AND is_active = true;

                    -- Return updated user information
                    RETURN QUERY
                    SELECT
                        u.id,
                        u.username::TEXT,
                        u.email::TEXT,
                        u.role,
                        u.password_hash::TEXT,
                        u.last_login_at,
                        u.refresh_token::TEXT,
                        u.refresh_token_expiry,
                        u.is_verified,
                        u.failed_login_attempts,
                        u.lockout_end
                    FROM users u
                    WHERE
                        u.email = p_email
                        AND u.is_active = true;
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to old stored procedure without lockout fields
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION public.get_and_update_user_login(
                    p_email TEXT,
                    p_refresh_token TEXT,
                    p_refresh_token_expiry TIMESTAMP WITH TIME ZONE
                )
                RETURNS TABLE (
                    id INTEGER,
                    username TEXT,
                    email TEXT,
                    role INTEGER,
                    password_hash TEXT,
                    last_login_at TIMESTAMP WITH TIME ZONE,
                    refresh_token TEXT,
                    refresh_token_expiry TIMESTAMP WITH TIME ZONE,
                    is_verified BOOLEAN
                )
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    -- Update user's last login timestamp and refresh token
                    UPDATE users
                    SET
                        last_login_at = CURRENT_TIMESTAMP,
                        refresh_token = p_refresh_token,
                        refresh_token_expiry = p_refresh_token_expiry,
                        updated_date = CURRENT_TIMESTAMP
                    WHERE
                        users.email = p_email
                        AND is_active = true;

                    -- Return updated user information
                    RETURN QUERY
                    SELECT
                        u.id,
                        u.username::TEXT,
                        u.email::TEXT,
                        u.role,
                        u.password_hash::TEXT,
                        u.last_login_at,
                        u.refresh_token::TEXT,
                        u.refresh_token_expiry,
                        u.is_verified
                    FROM users u
                    WHERE
                        u.email = p_email
                        AND u.is_active = true;
                END;
                $$;
            ");
        }
    }
}
