--drop table [ActiveSocialEvents]

if not exists(select 1 from sys.tables where name='ActiveSocialEvents')
begin

	create table [dbo].[ActiveSocialEvents](
		[ActiveSocialEventId] [int] identity(1,1) not null,
		[EventLogId] [int] not null,
		[SenderId] [int] not null,
		[EventDate] [datetime] not null,
		[SolutionName] [nvarchar](max) null,
		[LogType] [nvarchar](max) null,
		[BuildErrorLogId] [int] null,
		[ExecutionActionId] [int] null,
		[FirstName] [nvarchar](max) null,
		[LastName] [nvarchar](max) null,
		[InstitutionId] [int] not null,
		[MarkerType] [nvarchar](max) null,
		constraint [PK_dbo.ActiveSocialEventId] primary key clustered ([ActiveSocialEventId] asc),
		constraint [FK_dbo.ActiveSocialEvents_dbo.EventLogs_EventLogId] foreign key ([EventLogId]) references [dbo].[EventLogs] ([Id]),
		constraint [FK_dbo.ActiveSocialEvents_dbo.OsbideUsers_SenderId] foreign key ([SenderId]) references [dbo].[OsbideUsers] ([Id])
	)

end


if exists(select 1 from sys.indexes where name='IX_VisualizationQuery_ActiveSocialEvents_SenderId' and object_id=object_id('ActiveSocialEvents'))
	drop index [IX_VisualizationQuery_ActiveSocialEvents_SenderId] on [dbo].[ActiveSocialEvents]

create nonclustered index [IX_VisualizationQuery_ActiveSocialEvents_SenderId] on [dbo].[ActiveSocialEvents]([SenderId] asc)
include (EventDate, SolutionName, LogType, BuildErrorLogId, ExecutionActionId, FirstName, LastName, InstitutionId, MarkerType)
