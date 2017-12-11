
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/27/2017 00:38:42
-- Generated from EDMX file: C:\dev\source\auto_trade\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [trade];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[exchanges]', 'U') IS NOT NULL
    DROP TABLE [dbo].[exchanges];
GO
IF OBJECT_ID(N'[dbo].[markets]', 'U') IS NOT NULL
    DROP TABLE [dbo].[markets];
GO
IF OBJECT_ID(N'[dbo].[my_orders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[my_orders];
GO
IF OBJECT_ID(N'[dbo].[orders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[orders];
GO
IF OBJECT_ID(N'[dbo].[price_hist]', 'U') IS NOT NULL
    DROP TABLE [dbo].[price_hist];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'exchanges'
CREATE TABLE [dbo].[exchanges] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(100)  NULL,
    [key] nvarchar(500)  NULL
);
GO

-- Creating table 'markets'
CREATE TABLE [dbo].[markets] (
    [id] int IDENTITY(1,1) NOT NULL,
    [exchange_id] int  NULL,
    [currency] nvarchar(20)  NULL,
    [currency_long] nvarchar(50)  NULL,
    [base_currency] nvarchar(20)  NULL,
    [min_trade_size] decimal(18,8)  NULL,
    [market_name] nvarchar(20)  NULL,
    [is_active] bit  NULL,
    [created_at] datetime  NULL
);
GO

-- Creating table 'my_orders'
CREATE TABLE [dbo].[my_orders] (
    [id] int IDENTITY(1,1) NOT NULL,
    [market_id] int  NULL,
    [order_uuid] nvarchar(50)  NULL,
    [order_type] nvarchar(20)  NULL,
    [quantity] decimal(18,8)  NULL,
    [quantity_remain] decimal(18,8)  NULL,
    [reserved] decimal(18,8)  NULL,
    [reserved_remaining] decimal(18,8)  NULL,
    [commission_reserved] decimal(18,8)  NULL,
    [commission_reserved_remaining] decimal(18,8)  NULL,
    [commission_paid] decimal(18,8)  NULL,
    [price] decimal(18,8)  NULL,
    [opened_at] datetime  NULL,
    [closed_at] datetime  NULL
);
GO

-- Creating table 'orders'
CREATE TABLE [dbo].[orders] (
    [id] int IDENTITY(1,1) NOT NULL,
    [market_id] int  NULL,
    [type] bit  NULL,
    [quantity] decimal(18,8)  NULL,
    [rate] decimal(18,8)  NULL
);
GO

-- Creating table 'price_hist'
CREATE TABLE [dbo].[price_hist] (
    [id] int IDENTITY(1,1) NOT NULL,
    [market_id] int  NULL,
    [bid] decimal(18,8)  NULL,
    [ask] decimal(18,8)  NULL,
    [last] decimal(18,8)  NULL,
    [avg] decimal(18,8)  NULL,
    [acc_rate] int  NULL,
    [created_at] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'exchanges'
ALTER TABLE [dbo].[exchanges]
ADD CONSTRAINT [PK_exchanges]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'markets'
ALTER TABLE [dbo].[markets]
ADD CONSTRAINT [PK_markets]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'my_orders'
ALTER TABLE [dbo].[my_orders]
ADD CONSTRAINT [PK_my_orders]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'orders'
ALTER TABLE [dbo].[orders]
ADD CONSTRAINT [PK_orders]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'price_hist'
ALTER TABLE [dbo].[price_hist]
ADD CONSTRAINT [PK_price_hist]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------