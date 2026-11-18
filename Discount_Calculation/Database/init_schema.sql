CREATE DATABASE OrderDiscountDb;
GO
USE OrderDiscountDb;
GO
CREATE TABLE dbo.OrderDiscountCalculations
(
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId NVARCHAR(64) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerType NVARCHAR(50) NOT NULL,
    OriginalAmount DECIMAL(18, 4) NOT NULL,
    DiscountRate DECIMAL(9, 6) NOT NULL,
    DiscountAmount DECIMAL(18, 4) NOT NULL,
    FinalAmount DECIMAL(18, 4) NOT NULL,
    CalculatedAtUtc DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_OrderDiscountCalculations_OriginalAmount CHECK (OriginalAmount > 0),
    CONSTRAINT CK_OrderDiscountCalculations_DiscountRate CHECK (DiscountRate >= 0 AND DiscountRate <= 1),
    CONSTRAINT CK_OrderDiscountCalculations_FinalAmount CHECK (FinalAmount >= 0)
);
GO
CREATE INDEX IX_OrderDiscountCalculations_OrderId ON dbo.OrderDiscountCalculations (OrderId);
GO