-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- add new columns to [dbo].[BuildDocuments]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------

if not exists (select 1 from sys.tables t inner join sys.columns c on c.object_id=t.object_id where t.name='BuildDocuments' and c.name='NumberOfInserted')
	alter table [dbo].[BuildDocuments] add NumberOfInserted int null

if not exists (select 1 from sys.tables t inner join sys.columns c on c.object_id=t.object_id where t.name='BuildDocuments' and c.name='NumberOfModified')
	alter table [dbo].[BuildDocuments] add NumberOfModified int null

if not exists (select 1 from sys.tables t inner join sys.columns c on c.object_id=t.object_id where t.name='BuildDocuments' and c.name='NumberOfDeleted')
	alter table [dbo].[BuildDocuments] add NumberOfDeleted int null

if not exists (select 1 from sys.tables t inner join sys.columns c on c.object_id=t.object_id where t.name='BuildDocuments' and c.name='ModifiedLines')
	alter table [dbo].[BuildDocuments] add ModifiedLines nvarchar(max) null

if not exists (select 1 from sys.tables t inner join sys.columns c on c.object_id=t.object_id where t.name='BuildDocuments' and c.name='UpdatedOn')
	alter table [dbo].[BuildDocuments] add UpdatedOn datetime null

if not exists (select 1 from sys.tables t inner join sys.columns c on c.object_id=t.object_id where t.name='BuildDocuments' and c.name='UpdatedBy')
	alter table [dbo].[BuildDocuments] add UpdatedBy int null



/*
-- remove new columns from [dbo].[BuildDocuments]

alter table [dbo].[BuildDocuments] drop column NumberOfInserted
alter table [dbo].[BuildDocuments] drop column NumberOfModified
alter table [dbo].[BuildDocuments] drop column NumberOfDeleted
alter table [dbo].[BuildDocuments] drop column ModifiedLines
alter table [dbo].[BuildDocuments] drop column UpdatedOn
alter table [dbo].[BuildDocuments] drop column UpdatedBy

alter table [dbo].[BuildDocuments] drop constraint DF_BuildDocuments_Counted
alter table [dbo].[BuildDocuments] drop column Counted

*/
