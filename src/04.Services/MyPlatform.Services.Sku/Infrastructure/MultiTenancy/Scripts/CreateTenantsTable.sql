-- =============================================================================
-- Create Tenants Table for Multi-Tenancy Management
-- =============================================================================
-- This script creates the tenants table for storing tenant configuration.
-- 
-- This is an example implementation for MySQL. Modify the script based on your
-- database provider (PostgreSQL, SQL Server, etc.) and specific requirements.
--
-- Usage:
--   mysql -u root -p your_database < CreateTenantsTable.sql
-- =============================================================================

CREATE TABLE IF NOT EXISTS `tenants` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `tenant_id` VARCHAR(64) NOT NULL COMMENT 'Unique tenant identifier',
    `name` VARCHAR(256) NOT NULL COMMENT 'Display name of the tenant',
    `isolation_mode` VARCHAR(32) NOT NULL DEFAULT 'Shared' COMMENT 'Data isolation mode: Shared, Isolated',
    `connection_string` VARCHAR(1024) NULL COMMENT 'Connection string for isolated tenants',
    `status` VARCHAR(32) NOT NULL DEFAULT 'Active' COMMENT 'Tenant status: Active, Suspended, Deleted',
    `configuration` TEXT NULL COMMENT 'Additional configuration as JSON',
    `created_at` DATETIME NOT NULL COMMENT 'Creation timestamp',
    `updated_at` DATETIME NULL COMMENT 'Last update timestamp',
    `deleted_at` DATETIME NULL COMMENT 'Soft delete timestamp',
    
    PRIMARY KEY (`id`),
    UNIQUE INDEX `ix_tenants_tenant_id` (`tenant_id`),
    INDEX `ix_tenants_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tenant management table';

-- =============================================================================
-- Example: Insert sample tenants for development/testing
-- =============================================================================
-- Uncomment the following to insert sample data:
--
-- INSERT INTO `tenants` (`tenant_id`, `name`, `isolation_mode`, `status`, `created_at`)
-- VALUES 
--     ('tenant-001', 'Tenant One', 'Shared', 'Active', UTC_TIMESTAMP()),
--     ('tenant-002', 'Tenant Two', 'Shared', 'Active', UTC_TIMESTAMP()),
--     ('tenant-003', 'Tenant Three', 'Isolated', 'Active', UTC_TIMESTAMP());
