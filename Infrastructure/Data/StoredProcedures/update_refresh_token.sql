-- =====================================================
-- Stored Procedure: update_refresh_token
-- Description: Updates user's refresh token and returns last login timestamp
-- Parameters:
--   p_user_id: User ID
--   p_old_refresh_token: Current refresh token (for validation)
--   p_new_refresh_token: New refresh token to set
--   p_new_refresh_token_expiry: New refresh token expiry timestamp
-- Returns: Table with last_login_at timestamp
-- =====================================================

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

-- Grant execute permission to appropriate roles
-- GRANT EXECUTE ON FUNCTION public.update_refresh_token(INTEGER, TEXT, TEXT, TIMESTAMP WITH TIME ZONE) TO your_app_role;
