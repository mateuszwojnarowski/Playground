-- Seed schema and data for Module 08 (SQL read).
-- Run automatically by infra-local/scripts/seed-sql.sh after the container is healthy.
IF DB_ID('LearningDb') IS NULL
BEGIN
    CREATE DATABASE LearningDb;
END
GO

USE LearningDb;
GO

IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        Id   INT           NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Tier NVARCHAR(50)  NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    INSERT INTO dbo.Customers (Id, Name, Tier) VALUES
        (1, N'Ada Lovelace',    N'Gold'),
        (2, N'Alan Turing',     N'Platinum'),
        (3, N'Grace Hopper',    N'Silver'),
        (4, N'Edsger Dijkstra', N'Gold'),
        (5, N'Margaret Hamilton', N'Platinum');
END
GO
