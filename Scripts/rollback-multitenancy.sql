-- ============================================================================
-- MULTI-TENANCY ROLLBACK SCRIPT
-- Pastırma E-Commerce Platform
-- ============================================================================
-- This script reverts the multi-tenancy migration, moving all tables back
-- to the public schema.
--
-- USE THIS ONLY IF:
-- 1. Something went wrong during migration
-- 2. You want to revert to single-tenant architecture
-- ============================================================================

-- ============================================================================
-- STEP 1: MOVE TABLES BACK TO PUBLIC SCHEMA
-- ============================================================================

-- Users table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'users') THEN
        ALTER TABLE tenant_default.users SET SCHEMA public;
        RAISE NOTICE 'Restored: users';
    END IF;
END $$;

-- Products table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'products') THEN
        ALTER TABLE tenant_default.products SET SCHEMA public;
        RAISE NOTICE 'Restored: products';
    END IF;
END $$;

-- Product Images table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'product_images') THEN
        ALTER TABLE tenant_default.product_images SET SCHEMA public;
        RAISE NOTICE 'Restored: product_images';
    END IF;
END $$;

-- Categories table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'categories') THEN
        ALTER TABLE tenant_default.categories SET SCHEMA public;
        RAISE NOTICE 'Restored: categories';
    END IF;
END $$;

-- Orders table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'orders') THEN
        ALTER TABLE tenant_default.orders SET SCHEMA public;
        RAISE NOTICE 'Restored: orders';
    END IF;
END $$;

-- Order Items table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'order_items') THEN
        ALTER TABLE tenant_default.order_items SET SCHEMA public;
        RAISE NOTICE 'Restored: order_items';
    END IF;
END $$;

-- Reviews table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'reviews') THEN
        ALTER TABLE tenant_default.reviews SET SCHEMA public;
        RAISE NOTICE 'Restored: reviews';
    END IF;
END $$;

-- Favorites table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'favorites') THEN
        ALTER TABLE tenant_default.favorites SET SCHEMA public;
        RAISE NOTICE 'Restored: favorites';
    END IF;
END $$;

-- Hero Slides table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'hero_slides') THEN
        ALTER TABLE tenant_default.hero_slides SET SCHEMA public;
        RAISE NOTICE 'Restored: hero_slides';
    END IF;
END $$;

-- Blog Posts table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'blog_posts') THEN
        ALTER TABLE tenant_default.blog_posts SET SCHEMA public;
        RAISE NOTICE 'Restored: blog_posts';
    END IF;
END $$;

-- Blog Categories table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'blog_categories') THEN
        ALTER TABLE tenant_default.blog_categories SET SCHEMA public;
        RAISE NOTICE 'Restored: blog_categories';
    END IF;
END $$;

-- Addresses table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'addresses') THEN
        ALTER TABLE tenant_default.addresses SET SCHEMA public;
        RAISE NOTICE 'Restored: addresses';
    END IF;
END $$;

-- Notifications table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'notifications') THEN
        ALTER TABLE tenant_default.notifications SET SCHEMA public;
        RAISE NOTICE 'Restored: notifications';
    END IF;
END $$;

-- Contact Form Submissions table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'tenant_default' AND tablename = 'contact_form_submissions') THEN
        ALTER TABLE tenant_default.contact_form_submissions SET SCHEMA public;
        RAISE NOTICE 'Restored: contact_form_submissions';
    END IF;
END $$;

-- ============================================================================
-- STEP 2: RESTORE STORED PROCEDURES TO PUBLIC SCHEMA
-- ============================================================================
-- Note: The tenant schema will be dropped in Step 3, which will also drop the functions.
-- We need to recreate them in public schema first (while users table exists in public).

-- Recreate get_and_update_user_login in public schema
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
    UPDATE users
    SET
        last_login_at = CURRENT_TIMESTAMP,
        refresh_token = p_refresh_token,
        refresh_token_expiry = p_refresh_token_expiry,
        updated_date = CURRENT_TIMESTAMP
    WHERE
        users.email = p_email
        AND is_active = true;

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

SELECT 'Restored: public.get_and_update_user_login' as info;

-- Recreate update_refresh_token in public schema
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
    UPDATE users
    SET
        refresh_token = p_new_refresh_token,
        refresh_token_expiry = p_new_refresh_token_expiry,
        updated_date = CURRENT_TIMESTAMP
    WHERE
        id = p_user_id
        AND refresh_token = p_old_refresh_token
        AND is_active = true;

    RETURN QUERY
    SELECT u.last_login_at
    FROM users u
    WHERE u.id = p_user_id;
END;
$$;

SELECT 'Restored: public.update_refresh_token' as info;

-- ============================================================================
-- STEP 3: DROP TENANT SCHEMA (this also drops tenant-specific functions)
-- ============================================================================
DROP SCHEMA IF EXISTS tenant_default CASCADE;
SELECT 'Dropped: tenant_default schema' as info;

-- ============================================================================
-- STEP 4: DROP TENANTS TABLE
-- ============================================================================
DROP TABLE IF EXISTS public.tenants CASCADE;
SELECT 'Dropped: public.tenants table' as info;

-- ============================================================================
-- STEP 4: VERIFICATION
-- ============================================================================
SELECT '=== VERIFICATION ===' as section;

SELECT 'Tables restored to public schema:' as info;
SELECT tablename
FROM pg_tables
WHERE schemaname = 'public'
  AND tablename NOT LIKE 'pg_%'
ORDER BY tablename;

SELECT 'Schemas in database:' as info;
SELECT nspname
FROM pg_namespace
WHERE nspname NOT LIKE 'pg_%'
  AND nspname != 'information_schema';

-- ============================================================================
-- ROLLBACK COMPLETE!
-- ============================================================================
SELECT '============================================' as separator;
SELECT 'ROLLBACK COMPLETED SUCCESSFULLY!' as status;
SELECT 'Your data is back in: public schema' as info;
SELECT '============================================' as separator;
