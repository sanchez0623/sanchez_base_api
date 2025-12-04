-- =====================================================
-- Tenants Table Migration Script
-- This script creates the tenants table for storing
-- tenant information in the tenant management database.
-- =====================================================

-- =====================================================
-- MySQL Version
-- =====================================================

CREATE TABLE IF NOT EXISTS `tenants` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `tenant_id` VARCHAR(50) NOT NULL COMMENT 'Unique tenant identifier',
    `name` VARCHAR(200) NOT NULL COMMENT 'Tenant display name',
    `isolation_mode` VARCHAR(20) NOT NULL DEFAULT 'Shared' COMMENT 'Data isolation mode: Shared, Isolated',
    `connection_string` VARCHAR(500) NULL COMMENT 'Database connection string for isolated tenants',
    `status` VARCHAR(20) NOT NULL DEFAULT 'Active' COMMENT 'Tenant status: Active, Suspended, Deleted',
    `configuration` JSON NULL COMMENT 'Additional tenant configuration as JSON',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `deleted_at` DATETIME NULL,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `uk_tenant_id` (`tenant_id`),
    INDEX `idx_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tenant information table';

-- Insert default tenant
INSERT INTO `tenants` (`tenant_id`, `name`, `isolation_mode`, `connection_string`, `status`) VALUES
('default', 'Default Tenant', 'Shared', NULL, 'Active');


-- =====================================================
-- PostgreSQL Version (Alternative)
-- =====================================================
-- 
-- CREATE TABLE IF NOT EXISTS tenants (
--     id BIGSERIAL PRIMARY KEY,
--     tenant_id VARCHAR(50) NOT NULL,
--     name VARCHAR(200) NOT NULL,
--     isolation_mode VARCHAR(20) NOT NULL DEFAULT 'Shared',
--     connection_string VARCHAR(500) NULL,
--     status VARCHAR(20) NOT NULL DEFAULT 'Active',
--     configuration JSONB NULL,
--     created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP NULL,
--     deleted_at TIMESTAMP NULL,
--     CONSTRAINT uk_tenant_id UNIQUE (tenant_id)
-- );
-- 
-- CREATE INDEX IF NOT EXISTS idx_tenants_status ON tenants (status);
-- 
-- INSERT INTO tenants (tenant_id, name, isolation_mode, connection_string, status)
-- VALUES ('default', 'Default Tenant', 'Shared', NULL, 'Active')
-- ON CONFLICT (tenant_id) DO NOTHING;


-- =====================================================
-- SQL Server Version (Alternative)
-- =====================================================
-- 
-- IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tenants' AND xtype='U')
-- BEGIN
--     CREATE TABLE tenants (
--         id BIGINT IDENTITY(1,1) PRIMARY KEY,
--         tenant_id NVARCHAR(50) NOT NULL,
--         name NVARCHAR(200) NOT NULL,
--         isolation_mode NVARCHAR(20) NOT NULL DEFAULT 'Shared',
--         connection_string NVARCHAR(500) NULL,
--         status NVARCHAR(20) NOT NULL DEFAULT 'Active',
--         configuration NVARCHAR(MAX) NULL,
--         created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
--         updated_at DATETIME2 NULL,
--         deleted_at DATETIME2 NULL,
--         CONSTRAINT uk_tenant_id UNIQUE (tenant_id)
--     );
--     
--     CREATE INDEX idx_tenants_status ON tenants (status);
-- END;
-- 
-- IF NOT EXISTS (SELECT 1 FROM tenants WHERE tenant_id = 'default')
-- BEGIN
--     INSERT INTO tenants (tenant_id, name, isolation_mode, connection_string, status)
--     VALUES ('default', 'Default Tenant', 'Shared', NULL, 'Active');
-- END;
