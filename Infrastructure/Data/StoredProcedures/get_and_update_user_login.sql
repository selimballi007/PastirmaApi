-- =====================================================
-- Stored Procedure: get_and_update_user_login
-- Description: Retrieves user by email and updates login information
-- Parameters:
--   p_email: User's email address
--   p_refresh_token: New refresh token to set
--   p_refresh_token_expiry: New refresh token expiry timestamp
-- Returns: Table with user information
-- =====================================================

--DROP FUNCTION get_and_update_user_login(text,text,timestamp with time zone)

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

-- Grant execute permission to appropriate roles
-- GRANT EXECUTE ON FUNCTION public.get_and_update_user_login(TEXT, TEXT, TIMESTAMP WITH TIME ZONE) TO your_app_role;
