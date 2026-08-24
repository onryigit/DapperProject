-- TradePulse şeması. Veri üretimi uygulama başlangıcında DatabaseSeeder tarafından yapılır.
IF OBJECT_ID(N'dbo.TradeLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TradeLogs
    (
        Id INT NOT NULL CONSTRAINT PK_TradeLogs PRIMARY KEY CLUSTERED,
        UserCode NVARCHAR(20) NOT NULL,
        CryptoPair NVARCHAR(20) NOT NULL,
        TradeType NVARCHAR(4) NOT NULL,
        Price DECIMAL(20,8) NOT NULL,
        Quantity DECIMAL(20,8) NOT NULL,
        TotalUSD DECIMAL(20,4) NOT NULL,
        FeeUSD DECIMAL(20,4) NOT NULL,
        LocationCountry NVARCHAR(60) NOT NULL,
        ExecutionTimeMs INT NOT NULL,
        TransactionDate DATETIME NOT NULL,
        CONSTRAINT CK_TradeLogs_TradeType CHECK (TradeType IN (N'BUY', N'SELL'))
    );
END;

IF OBJECT_ID(N'dbo.SeedHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SeedHistory
    (
        SeedKey NVARCHAR(100) NOT NULL CONSTRAINT PK_SeedHistory PRIMARY KEY,
        RecordCount INT NOT NULL,
        CompletedAt DATETIME2 NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_TransactionDate' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
    CREATE NONCLUSTERED INDEX IX_TradeLogs_TransactionDate ON dbo.TradeLogs (TransactionDate DESC, Id DESC)
    INCLUDE (CryptoPair, TradeType, TotalUSD, FeeUSD, ExecutionTimeMs);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_CryptoPair' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
    CREATE NONCLUSTERED INDEX IX_TradeLogs_CryptoPair ON dbo.TradeLogs (CryptoPair)
    INCLUDE (TradeType, TotalUSD, TransactionDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_TradeType' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
    CREATE NONCLUSTERED INDEX IX_TradeLogs_TradeType ON dbo.TradeLogs (TradeType) INCLUDE (TotalUSD);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_TotalUSD' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
    CREATE NONCLUSTERED INDEX IX_TradeLogs_TotalUSD ON dbo.TradeLogs (TotalUSD DESC, Id DESC)
    INCLUDE (UserCode, CryptoPair, TradeType, Price, Quantity, FeeUSD, LocationCountry, ExecutionTimeMs, TransactionDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_LocationCountry' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
    CREATE NONCLUSTERED INDEX IX_TradeLogs_LocationCountry ON dbo.TradeLogs (LocationCountry) INCLUDE (TotalUSD);
