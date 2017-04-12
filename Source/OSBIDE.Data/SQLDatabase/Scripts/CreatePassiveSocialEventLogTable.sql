
create table [dbo].[PassiveSocialEventProcessLog]
(
	[Id] [int] identity(1,1) not null,
	[SourceTableName] [nvarchar](256) not null,
	[CreatedDate] [datetime] not null,
	[DestinationTableName] [nvarchar](256) null,
	[LastProcessedDate] [datetime] null,
	[ProcessCompleted] [bit] null,
	[ProcessedRecordCounts] [int] null,
	constraint [PK_dbo.PassiveSocialEventProcessLog] primary key clustered ([Id] asc),
	constraint [U_SourceTableName] unique ([SourceTableName])
)
go

create procedure [dbo].[InsertPassiveSocialEventProcessLog]

	 @tableName nvarchar(256)
as
begin

	set nocount on;

	if not exists(select 1 from [dbo].[PassiveSocialEventProcessLog] where [SourceTableName]=@tableName)
	insert into [dbo].[PassiveSocialEventProcessLog] ([SourceTableName], [CreatedDate]) values (@tableName, getDate())

end
go

create procedure [dbo].[GetPassiveSocialEventProcessLog]
as
begin

	set nocount on;

	select [Id], [SourceTableName], [CreatedDate], [LastProcessedDate], [ProcessedRecordCounts]
	from [dbo].[PassiveSocialEventProcessLog]
	where isNull([ProcessCompleted], 0)=0

end
go

create procedure [dbo].[UpdatePassiveSocialEventProcessLog]

	 @id int
	,@destTableName nvarchar(256)
	,@completed bit
	,@processedRecordCounts int
as
begin

	set nocount on;

	update [dbo].[PassiveSocialEventProcessLog]
	set [DestinationTableName]=@destTableName, [LastProcessedDate]=getDate(), [ProcessedRecordCounts]=@processedRecordCounts,[ProcessCompleted]=@completed
	where [Id]=@id

end
go

