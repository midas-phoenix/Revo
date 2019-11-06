﻿-- Revo.Extensions.History SQL baseline schema for common providers (EF Core, EF6)
-- MSSQL version

-- HISTORY

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RHI_TRACKED_CHANGE_RECORD')
CREATE TABLE [dbo].[RHI_TRACKED_CHANGE_RECORD] (
	[RHI_TCH_TrackedChangeRecordId] [UNIQUEIDENTIFIER] NOT NULL,
	[RHI_TCH_Ordinal] [INT] IDENTITY(1,1) NOT NULL,
	[RHI_TCH_ActorName] [NVARCHAR] (MAX) NOT NULL,
	[RHI_TCH_UserId] [UNIQUEIDENTIFIER],
	[RHI_TCH_AggregateId] [UNIQUEIDENTIFIER],
	[RHI_TCH_AggregateClassId] [UNIQUEIDENTIFIER],
	[RHI_TCH_EntityId] [UNIQUEIDENTIFIER],
	[RHI_TCH_EntityClassId] [UNIQUEIDENTIFIER],
	[RHI_TCH_ChangeTime] [DATETIMEOFFSET] NOT NULL,
	[RHI_TCH_ChangeDataJson] [NVARCHAR] (MAX) NOT NULL,
	[RHI_TCH_ChangeDataClassName] [NVARCHAR] (MAX) NOT NULL,
	CONSTRAINT [RHI_TRACKED_CHANGE_RECORD_PK] PRIMARY KEY NONCLUSTERED ([RHI_TCH_TrackedChangeRecordId])/*,
	CONSTRAINT [RHI_TRACKED_CHANGE_RECORD_FK_USERID] FOREIGN KEY ([RHI_TCH_UserId]) REFERENCES [dbo].[GT_USER] ([GT_USR_UserId])*/
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RHI_ENTITY_ATTRIBUTE_DATA')
CREATE TABLE [dbo].[RHI_ENTITY_ATTRIBUTE_DATA] (
	[RHI_EAD_EntityAttributeDataId] [UNIQUEIDENTIFIER] NOT NULL,
	[RHI_EAD_Ordinal] [INT] IDENTITY(1,1) NOT NULL,
	[RHI_EAD_AggregateId] [UNIQUEIDENTIFIER],
	[RHI_EAD_EntityId] [UNIQUEIDENTIFIER],
	[RHI_EAD_AttributeValueMapJson] [NVARCHAR] (MAX) NOT NULL,
	[RHI_EAD_Version] [INT] NOT NULL,
	CONSTRAINT [RHI_ENTITY_ATTRIBUTE_DATA_PK] PRIMARY KEY NONCLUSTERED ([RHI_EAD_EntityAttributeDataId])
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'CIX_RHI_TRACKED_CHANGE_RECORD')
	CREATE CLUSTERED INDEX [CIX_RHI_TRACKED_CHANGE_RECORD] ON [dbo].[RHI_TRACKED_CHANGE_RECORD] ([RHI_TCH_Ordinal]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'CIX_RHI_ENTITY_ATTRIBUTE_DATA')
	CREATE CLUSTERED INDEX [CIX_RHI_ENTITY_ATTRIBUTE_DATA] ON [dbo].[RHI_ENTITY_ATTRIBUTE_DATA] ([RHI_EAD_Ordinal]);
GO