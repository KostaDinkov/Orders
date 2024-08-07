
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE TABLE [Clients] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [DiscountPercent] int NOT NULL,
        [IsCompany] bit NOT NULL,
        [IsSpecialPrice] bit NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE TABLE [Products] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [PriceDrebno] decimal(18,2) NOT NULL,
        [PriceEdro] decimal(18,2) NOT NULL,
        [HasDiscount] bit NOT NULL,
        [KeepPriceDrebno] bit NOT NULL,
        [inPriceList] bit NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NULL,
        [DateCreated] datetime2 NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [OperatorId] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [PickupDate] datetime2 NOT NULL,
        [ClientName] nvarchar(max) NOT NULL,
        [ClientPhone] nvarchar(max) NOT NULL,
        [ClientId] int NULL,
        [IsPaid] bit NOT NULL,
        [AdvancePaiment] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] uniqueidentifier NOT NULL,
        [ProductAmount] float NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CakeFoto] nvarchar(max) NOT NULL,
        [CakeTitle] nvarchar(max) NOT NULL,
        [IsInProgress] bit NOT NULL,
        [IsComplete] bit NOT NULL,
        [ItemUnitPrice] decimal(18,2) NOT NULL,
        [OrderId] int NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    CREATE INDEX [IX_Orders_ClientId] ON [Orders] ([ClientId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230326155946_Initial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230326155946_Initial', N'7.0.2');
END;
GO

COMMIT;
GO


