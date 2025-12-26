using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastirmaApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing stored procedures if they exist (with any parameter names)
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.update_refresh_token(INTEGER, TEXT, TEXT, TIMESTAMP WITH TIME ZONE);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.get_and_update_user_login(TEXT, TEXT, TIMESTAMP WITH TIME ZONE);");

            // Create update_refresh_token stored procedure
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION public.update_refresh_token(
                    p_user_id INTEGER,
                    p_old_refresh_token TEXT,
                    p_new_refresh_token TEXT,
                    p_new_refresh_token_expiry TIMESTAMP WITH TIME ZONE
                )
                RETURNS TABLE (
                    last_login_at TIMESTAMP WITH TIME ZONE
                )
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    -- Update the user's refresh token and expiry
                    UPDATE users
                    SET
                        refresh_token = p_new_refresh_token,
                        refresh_token_expiry = p_new_refresh_token_expiry,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE
                        id = p_user_id
                        AND refresh_token = p_old_refresh_token
                        AND is_active = true;

                    -- Return the last_login_at timestamp
                    RETURN QUERY
                    SELECT u.last_login_at
                    FROM users u
                    WHERE u.id = p_user_id;
                END;
                $$;
            ");

            // Create get_and_update_user_login stored procedure
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
                    role TEXT,
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
                        updated_at = CURRENT_TIMESTAMP
                    WHERE
                        users.email = p_email
                        AND is_active = true;

                    -- Return updated user information
                    RETURN QUERY
                    SELECT
                        u.id,
                        u.username,
                        u.email,
                        u.role,
                        u.password_hash,
                        u.last_login_at,
                        u.refresh_token,
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop stored procedures
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.update_refresh_token(INTEGER, TEXT, TEXT, TIMESTAMP WITH TIME ZONE);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.get_and_update_user_login(TEXT, TEXT, TIMESTAMP WITH TIME ZONE);");
        }
    }
}
