-- ============================================================================
-- MULTI-TENANCY DATA MIGRATION SCRIPT
-- Pastırma E-Commerce Platform
-- ============================================================================
-- This script migrates existing data from public schema to tenant schema
-- for "Shared Database, Separate Schema" multi-tenancy architecture.
--
-- IMPORTANT:
-- 1. BACKUP YOUR DATABASE BEFORE RUNNING THIS SCRIPT!
-- 2. Run this script during a maintenance window (low traffic)
-- 3. Test on a local/staging database first
-- ============================================================================

-- ============================================================================
-- STEP 0: CONFIGURATION
-- ============================================================================
-- Change this to your desired tenant identifier
-- Example: 'default', 'pastirma', 'acme', etc.
\set tenant_name 'default'
\set schema_name 'tenant_default'

-- ============================================================================
-- STEP 1: CREATE BACKUP VERIFICATION POINT
-- ============================================================================
-- Run this command BEFORE the script to create a backup:
-- pg_dump -h your-host -U your-user -d pastirma_db -F c -f backup_before_multitenancy.dump
--
-- Or for Railway:
-- pg_dump $DATABASE_URL -F c -f backup_before_multitenancy.dump

-- Verify current state
SELECT 'Current tables in public schema:' as info;
SELECT schemaname, tablename
FROM pg_tables
WHERE schemaname = 'public'
  AND tablename NOT LIKE 'pg_%'
ORDER BY tablename;

-- ============================================================================
-- STEP 2: CREATE TENANT SCHEMA
-- ============================================================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'tenant_default') THEN
        CREATE SCHEMA tenant_default;
        RAISE NOTICE 'Schema tenant_default created successfully';
    ELSE
        RAISE NOTICE 'Schema tenant_default already exists';
    END IF;
END $$;

-- ============================================================================
-- STEP 3: MOVE APPLICATION TABLES TO TENANT SCHEMA
-- ============================================================================
-- Note: ALTER TABLE SET SCHEMA is a metadata-only operation - very fast, no data copy

-- Users table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'users') THEN
        ALTER TABLE public.users SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: users';
    END IF;
END $$;

-- Products table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'products') THEN
        ALTER TABLE public.products SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: products';
    END IF;
END $$;

-- Product Images table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'product_images') THEN
        ALTER TABLE public.product_images SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: product_images';
    END IF;
END $$;

-- Categories table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'categories') THEN
        ALTER TABLE public.categories SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: categories';
    END IF;
END $$;

-- Orders table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'orders') THEN
        ALTER TABLE public.orders SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: orders';
    END IF;
END $$;

-- Order Items table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'order_items') THEN
        ALTER TABLE public.order_items SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: order_items';
    END IF;
END $$;

-- Reviews table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'reviews') THEN
        ALTER TABLE public.reviews SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: reviews';
    END IF;
END $$;

-- Favorites table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'favorites') THEN
        ALTER TABLE public.favorites SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: favorites';
    END IF;
END $$;

-- Hero Slides table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'hero_slides') THEN
        ALTER TABLE public.hero_slides SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: hero_slides';
    END IF;
END $$;

-- Blog Posts table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'blog_posts') THEN
        ALTER TABLE public.blog_posts SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: blog_posts';
    END IF;
END $$;

-- Blog Categories table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'blog_categories') THEN
        ALTER TABLE public.blog_categories SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: blog_categories';
    END IF;
END $$;

-- Addresses table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'addresses') THEN
        ALTER TABLE public.addresses SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: addresses';
    END IF;
END $$;

-- Notifications table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'notifications') THEN
        ALTER TABLE public.notifications SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: notifications';
    END IF;
END $$;

-- Contact Form Submissions table
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'contact_form_submissions') THEN
        ALTER TABLE public.contact_form_submissions SET SCHEMA tenant_default;
        RAISE NOTICE 'Moved: contact_form_submissions';
    END IF;
END $$;

-- ============================================================================
-- STEP 4: MIGRATE STORED PROCEDURES TO TENANT SCHEMA
-- ============================================================================
-- Stored procedures reference the users table, so they need to be in tenant schema

-- Drop old functions from public schema (if they exist)
DROP FUNCTION IF EXISTS public.get_and_update_user_login(TEXT, TEXT, TIMESTAMP WITH TIME ZONE);
DROP FUNCTION IF EXISTS public.update_refresh_token(INTEGER, TEXT, TEXT, TIMESTAMP WITH TIME ZONE);

SELECT 'Dropped old stored procedures from public schema' as info;

-- Create get_and_update_user_login in tenant schema
CREATE OR REPLACE FUNCTION tenant_default.get_and_update_user_login(
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
    UPDATE tenant_default.users
    SET
        last_login_at = CURRENT_TIMESTAMP,
        refresh_token = p_refresh_token,
        refresh_token_expiry = p_refresh_token_expiry,
        updated_date = CURRENT_TIMESTAMP
    WHERE
        tenant_default.users.email = p_email
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
    FROM tenant_default.users u
    WHERE
        u.email = p_email
        AND u.is_active = true;
END;
$$;

SELECT 'Created: tenant_default.get_and_update_user_login' as info;

-- Create update_refresh_token in tenant schema
CREATE OR REPLACE FUNCTION tenant_default.update_refresh_token(
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
    UPDATE tenant_default.users
    SET
        refresh_token = p_new_refresh_token,
        refresh_token_expiry = p_new_refresh_token_expiry,
        updated_date = CURRENT_TIMESTAMP
    WHERE
        id = p_user_id
        AND refresh_token = p_old_refresh_token
        AND is_active = true;

    -- Return the last_login_at timestamp
    RETURN QUERY
    SELECT u.last_login_at
    FROM tenant_default.users u
    WHERE u.id = p_user_id;
END;
$$;

SELECT 'Created: tenant_default.update_refresh_token' as info;

-- ============================================================================
-- STEP 5: HANDLE EF CORE MIGRATIONS HISTORY
-- ============================================================================
-- Option A: Keep in public schema (shared migrations tracking)
-- The __EFMigrationsHistory table stays in public schema
-- This is fine if all tenants share the same migration history

-- Option B: Copy to tenant schema (per-tenant migrations tracking)
-- Uncomment the following if you want per-tenant migration tracking:
/*
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = '__EFMigrationsHistory') THEN
        -- Create a copy in tenant schema
        CREATE TABLE tenant_default."__EFMigrationsHistory" AS
        SELECT * FROM public."__EFMigrationsHistory";

        -- Add primary key
        ALTER TABLE tenant_default."__EFMigrationsHistory"
        ADD PRIMARY KEY ("MigrationId");

        RAISE NOTICE 'Copied: __EFMigrationsHistory';
    END IF;
END $$;
*/

-- ============================================================================
-- STEP 5: CREATE TENANT REGISTRY TABLE IN PUBLIC SCHEMA
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    identifier VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100),
    connection_string TEXT,
    schema_name VARCHAR(50) NOT NULL,
    domain VARCHAR(100) UNIQUE NOT NULL,
    site_name VARCHAR(100) DEFAULT 'Pastırma',
    site_description TEXT,
    logo_url TEXT,
    primary_color VARCHAR(7) DEFAULT '#DC2626',
    secondary_color VARCHAR(7) DEFAULT '#F97316',
    cloudinary_folder VARCHAR(100),
    turnstile_site_key VARCHAR(100),
    turnstile_secret_key VARCHAR(100),
    resend_api_key VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

-- Create indexes for fast lookup
CREATE INDEX IF NOT EXISTS idx_tenants_identifier ON public.tenants(identifier);
CREATE INDEX IF NOT EXISTS idx_tenants_domain ON public.tenants(domain);
CREATE INDEX IF NOT EXISTS idx_tenants_is_active ON public.tenants(is_active);

RAISE NOTICE 'Created: tenants table in public schema';

-- ============================================================================
-- STEP 6: REGISTER YOUR FIRST TENANT (YOUR EXISTING DATA)
-- ============================================================================
-- Update these values according to your setup
INSERT INTO public.tenants (
    identifier,
    name,
    schema_name,
    domain,
    site_name,
    site_description,
    logo_url,
    primary_color,
    secondary_color,
    cloudinary_folder,
    turnstile_site_key,
    is_active
) VALUES (
    'default',
    'Pastırma',
    'tenant_default',
    'devexer.uk',                    -- Your main domain
    'Pastırma',
    'Türkiye''nin en kaliteli pastırma ve şarküteri ürünlerini keşfedin.',
    '',                               -- Add your logo URL later
    '#DC2626',
    '#F97316',
    'tenants/default',
    '',                               -- Add your Turnstile site key
    true
) ON CONFLICT (identifier) DO NOTHING;

SELECT 'Registered tenant: default' as info;

-- ============================================================================
-- STEP 7: VERIFICATION
-- ============================================================================
SELECT '=== VERIFICATION ===' as section;

-- Check tables moved to tenant schema
SELECT 'Tables in tenant_default schema:' as info;
SELECT tablename
FROM pg_tables
WHERE schemaname = 'tenant_default'
ORDER BY tablename;

-- Check tenant registry
SELECT 'Registered tenants:' as info;
SELECT identifier, domain, schema_name, is_active
FROM public.tenants;

-- Check data integrity (row counts)
SELECT 'Data verification (row counts):' as info;

SELECT 'users' as table_name, COUNT(*) as row_count FROM tenant_default.users
UNION ALL
SELECT 'products', COUNT(*) FROM tenant_default.products
UNION ALL
SELECT 'categories', COUNT(*) FROM tenant_default.categories
UNION ALL
SELECT 'orders', COUNT(*) FROM tenant_default.orders
UNION ALL
SELECT 'reviews', COUNT(*) FROM tenant_default.reviews
UNION ALL
SELECT 'hero_slides', COUNT(*) FROM tenant_default.hero_slides;

-- Check stored procedures in tenant schema
SELECT 'Stored procedures in tenant_default schema:' as info;
SELECT routine_name, routine_type
FROM information_schema.routines
WHERE routine_schema = 'tenant_default'
ORDER BY routine_name;

-- ============================================================================
-- STEP 8: REMAINING TABLES IN PUBLIC SCHEMA
-- ============================================================================
SELECT 'Remaining tables in public schema (should only be tenants and EF migrations):' as info;
SELECT tablename
FROM pg_tables
WHERE schemaname = 'public'
  AND tablename NOT LIKE 'pg_%'
ORDER BY tablename;

-- ============================================================================
-- MIGRATION COMPLETE!
-- ============================================================================
SELECT '============================================' as separator;
SELECT 'MIGRATION COMPLETED SUCCESSFULLY!' as status;
SELECT 'Your existing data is now in: tenant_default schema' as info;
SELECT 'Tenant registry created in: public.tenants' as info;
SELECT '============================================' as separator;

-- ============================================================================
-- ROLLBACK SCRIPT (SAVE THIS SEPARATELY!)
-- ============================================================================
-- If you need to rollback, run these commands:
/*
-- Move tables back to public schema
ALTER TABLE tenant_default.users SET SCHEMA public;
ALTER TABLE tenant_default.products SET SCHEMA public;
ALTER TABLE tenant_default.product_images SET SCHEMA public;
ALTER TABLE tenant_default.categories SET SCHEMA public;
ALTER TABLE tenant_default.orders SET SCHEMA public;
ALTER TABLE tenant_default.order_items SET SCHEMA public;
ALTER TABLE tenant_default.reviews SET SCHEMA public;
ALTER TABLE tenant_default.favorites SET SCHEMA public;
ALTER TABLE tenant_default.hero_slides SET SCHEMA public;
ALTER TABLE tenant_default.blog_posts SET SCHEMA public;
ALTER TABLE tenant_default.blog_categories SET SCHEMA public;
ALTER TABLE tenant_default.addresses SET SCHEMA public;
ALTER TABLE tenant_default.notifications SET SCHEMA public;
ALTER TABLE tenant_default.contact_form_submissions SET SCHEMA public;

-- Drop tenant schema
DROP SCHEMA IF EXISTS tenant_default;

-- Drop tenants table
DROP TABLE IF EXISTS public.tenants;

SELECT 'ROLLBACK COMPLETED' as status;
*/
