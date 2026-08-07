-- TradePulse schema. Uygulama aynı idempotent şemayı DatabaseSeeder ile otomatik uygular.
CREATE TABLE dbo.TradeLogs
(
    Id INT NOT NULL CONSTRAINT PK_TradeLogs PRIMARY KEY CLUSTERED,
    UserCode NVARCHAR(20) NOT NULL,
    CryptoPair NVARCHAR(20) NOT NULL,
    TradeType NVARCHAR(4) NOT NULL CONSTRAINT CK_TradeLogs_TradeType CHECK (TradeType IN (N'BUY', N'SELL')),
    Price DECIMAL(20,8) NOT NULL,
    Quantity DECIMAL(20,8) NOT NULL,
    TotalUSD DECIMAL(20,4) NOT NULL,
    FeeUSD DECIMAL(20,4) NOT NULL,
    LocationCountry NVARCHAR(60) NOT NULL,
    ExecutionTimeMs INT NOT NULL,
    TransactionDate DATETIME NOT NULL
);

CREATE NONCLUSTERED INDEX IX_TradeLogs_TransactionDate ON dbo.TradeLogs (TransactionDate DESC, Id DESC)
INCLUDE (CryptoPair, TradeType, TotalUSD, FeeUSD, ExecutionTimeMs);
CREATE NONCLUSTERED INDEX IX_TradeLogs_CryptoPair ON dbo.TradeLogs (CryptoPair)
INCLUDE (TradeType, TotalUSD, TransactionDate);
CREATE NONCLUSTERED INDEX IX_TradeLogs_TradeType ON dbo.TradeLogs (TradeType) INCLUDE (TotalUSD);
CREATE NONCLUSTERED INDEX IX_TradeLogs_TotalUSD ON dbo.TradeLogs (TotalUSD DESC, Id DESC)
INCLUDE (UserCode, CryptoPair, TradeType, Price, Quantity, FeeUSD, LocationCountry, ExecutionTimeMs, TransactionDate);
CREATE NONCLUSTERED INDEX IX_TradeLogs_LocationCountry ON dbo.TradeLogs (LocationCountry) INCLUDE (TotalUSD);
