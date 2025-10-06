
-- Script de criação do schema do banco de dados
-- Sistema Challenge FIAP


USE [master]
GO


IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SystemChallengeFIAP')
BEGIN
    CREATE DATABASE [SystemChallengeFIAP]
END
GO

USE [SystemChallengeFIAP]
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [IdUser] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
        [FullName] [nvarchar](200) NOT NULL,
        [Email] [nvarchar](200) NOT NULL,
        [Password] [nvarchar](500) NOT NULL,
        [Document] [nvarchar](20) NOT NULL,
        [Role] [nvarchar](50) NOT NULL,
        [StatusAccount] [bit] NOT NULL DEFAULT (1),
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([IdUser] ASC)
    )
END
GO

-- Índices únicos para Users
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = N'IX_Users_Email')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = N'IX_Users_Document')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Document] ON [dbo].[Users] ([Document] ASC)
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Students](
        [IdStudent] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
        [IdUser] [uniqueidentifier] NOT NULL,
        [RegistrationNumber] [nvarchar](20) NOT NULL,
        [FullName] [nvarchar](200) NOT NULL,
        [Cpf] [nvarchar](14) NOT NULL,
        [BirthDate] [datetime2](7) NULL,
        [Address] [nvarchar](200) NULL,
        [PhoneNumber] [nvarchar](15) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        
        CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED ([IdStudent] ASC),
        CONSTRAINT [FK_Students_Users_IdUser] FOREIGN KEY([IdUser]) REFERENCES [dbo].[Users] ([IdUser]) ON DELETE CASCADE
    )
END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = N'IX_Students_RegistrationNumber')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Students_RegistrationNumber] ON [dbo].[Students] ([RegistrationNumber] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = N'IX_Students_Cpf')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Students_Cpf] ON [dbo].[Students] ([Cpf] ASC)
END
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Classes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Classes](
        [IdClass] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
        [ClassCode] [nvarchar](20) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Capacity] [int] NOT NULL DEFAULT (50),
        [Room] [nvarchar](50) NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT ('Open'),
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        
        CONSTRAINT [PK_Classes] PRIMARY KEY CLUSTERED ([IdClass] ASC)
    )
END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Classes]') AND name = N'IX_Classes_ClassCode')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Classes_ClassCode] ON [dbo].[Classes] ([ClassCode] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Enrollments](
        [IdEnrollment] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
        [IdStudent] [uniqueidentifier] NOT NULL,
        [IdClass] [uniqueidentifier] NOT NULL,
        [EnrollmentDate] [date] NOT NULL DEFAULT (CAST(GETUTCDATE() AS DATE)),
        [Status] [nvarchar](20) NOT NULL DEFAULT ('Active'),
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        
        CONSTRAINT [PK_Enrollments] PRIMARY KEY CLUSTERED ([IdEnrollment] ASC),
        CONSTRAINT [FK_Enrollments_Students_IdStudent] FOREIGN KEY([IdStudent]) REFERENCES [dbo].[Students] ([IdStudent]) ON DELETE CASCADE,
        CONSTRAINT [FK_Enrollments_Classes_IdClass] FOREIGN KEY([IdClass]) REFERENCES [dbo].[Classes] ([IdClass]) ON DELETE SET NULL
    )
END
GO


PRINT 'Schema do banco de dados criado com sucesso!'
PRINT 'Tabelas criadas:'
PRINT '- Users'
PRINT '- Students' 
PRINT '- Classes'
PRINT '- Enrollments'
PRINT ''
PRINT 'Índices únicos criados:'
PRINT '- IX_Users_Email'
PRINT '- IX_Users_Document'
PRINT '- IX_Students_RegistrationNumber'
PRINT '- IX_Students_Cpf'
PRINT '- IX_Classes_ClassCode'
GO
