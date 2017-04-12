-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- alter EventLogs table
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------

if not exists(select 1 from sys.columns c inner join sys.tables t on t.object_id=c.object_id and t.name='EventLogs' and c.name='EventTypeId')
begin

	alter table [dbo].[EventLogs] add EventTypeId int

	update [dbo].[EventLogs] set EventTypeId=et.EventTypeId
	from [dbo].[EventTypes] et
	inner join [dbo].[EventLogs] e on e.LogType=et.EventTypeName

	alter table [dbo].[EventLogs] alter column EventTypeId int not null

	create nonclustered index [IX_EventTypeId_DateRecieved_SenderId] on [dbo].[EventLogs]([SenderId] asc, [DateReceived] asc, [EventTypeId] asc)
	include(Id, LogType)

end

--select count(*) from [dbo].[EventLogs] where eventTypeId is null

if not exists(select 1 from sys.tables where name='EventTypes')
begin

	create table [dbo].[EventTypes] (
		[EventTypeId] [int] NOT NULL,
		[EventTypeName] [varchar](50) NOT NULL,
		[IsSocialEvent] [bit] NOT NULL,
		[IsIDEEvent] [bit] NOT NULL,
		[IsFeedEvent] [bit] NOT NULL,
		[IsEditEvent] [bit] NULL,
		[EventTypeCategoryId] [int] NULL
	)

	alter table [dbo].[EventTypes] add constraint PK_EventTypes primary key(EventTypeId)

	insert into [dbo].[EventTypes]
	([EventTypeId],[EventTypeName],[IsSocialEvent],[IsIDEEvent],[IsFeedEvent],[IsEditEvent],[EventTypeCategoryId]) values
	 (1,	'AskForHelpEvent',		1,	0,	1,	0,	1)
	,(2,	'BuildEvent',			0,	1,	1,	0,	NULL)
	,(3,	'CutCopyPasteEvent',	0,	0,	0,	1,	2)
	,(4,	'DebugEvent',			0,	0,	0,	0,	3)
	,(5,	'EditorActivityEvent',	0,	0,	0,	1,	2)
	,(6,	'ExceptionEvent',		0,	1,	1,	1,	2)
	,(7,	'FeedPostEvent',		1,	0,	1,	0,	1)
	,(8,	'HelpfulMarkGivenEvent',1,	0,	1,	0,	1)
	,(9,	'LogCommentEvent',		1,	0,	1,	0,	1)
	,(10,	'SaveEvent',			0,	0,	0,	1,	2)
	,(11,	'SubmitEvent',			1,	0,	1,	1,	2)

end

if exists(select 1 from sys.indexes where name='IX_FeedQuery_EventDate_EventLogId' and object_id=object_id('SaveEvents'))
	drop index [IX_FeedQuery_EventDate_EventLogId] on [dbo].[SaveEvents]

create nonclustered index [IX_FeedQuery_EventDate_EventLogId] on [dbo].[SaveEvents]([EventDate] asc)
include ([EventLogId])

if exists(select 1 from sys.indexes where name='IX_FeedQuery_UserID_Role_DefaultCourseId' and object_id=object_id('OsbideUsers'))
	drop index [IX_FeedQuery_UserID_Role_DefaultCourseId] on [dbo].[OsbideUsers]

create nonclustered index [IX_FeedQuery_UserID_Role_DefaultCourseId] on [dbo].[OsbideUsers]([Id] asc)
include ([DefaultCourseId])

if exists(select 1 from sys.indexes where name='IX_LogCommentEvents_SourceEventLogIdAll' and object_id=object_id('LogCommentEvents'))
	drop index [IX_LogCommentEvents_SourceEventLogIdAll] on [dbo].[LogCommentEvents]

create nonclustered index [IX_LogCommentEvents_SourceEventLogIdAll] on [dbo].[LogCommentEvents]([SourceEventLogId] asc)
include ([Id], [EventLogId], [EventDate], [SolutionName], [Content])

if exists(select 1 from sys.indexes where name='IX_LogCommentEvents_EventLogIdSourceId' and object_id=object_id('LogCommentEvents'))
	drop index [IX_LogCommentEvents_EventLogIdSourceId] on [dbo].[LogCommentEvents]

create nonclustered index [IX_LogCommentEvents_EventLogIdSourceId] on [dbo].[LogCommentEvents]([EventLogId] asc)
include ([SourceEventLogId])

if exists(select 1 from sys.indexes where name='IX_LogCommentEvents_EventLogId' and object_id=object_id('LogCommentEvents'))
	drop index [IX_LogCommentEvents_EventLogId] on [dbo].[LogCommentEvents]

create nonclustered index [IX_LogCommentEvents_EventLogId] on [dbo].[LogCommentEvents]([EventLogId] asc)
include ([Id],[SourceEventLogId],[EventDate],[SolutionName],[Content])

if exists(select 1 from sys.indexes where name='IX_ExceptionEvents_EventLogId' and object_id=object_id('ExceptionEvents'))
	drop index [IX_ExceptionEvents_EventLogId] on [dbo].[ExceptionEvents]

create nonclustered index [IX_ExceptionEvents_EventLogId] on [dbo].[ExceptionEvents]([EventLogId] asc)
include ([Id],[EventDate],[SolutionName],[ExceptionType],[ExceptionName],[ExceptionCode],[ExceptionDescription],[ExceptionAction],[DocumentName],[LineNumber],[LineContent])

if exists(select 1 from sys.indexes where name='IX_FeedPostEvents_EventLogId' and object_id=object_id('FeedPostEvents'))
	drop index [IX_FeedPostEvents_EventLogId] on [dbo].[FeedPostEvents]

create nonclustered index [IX_FeedPostEvents_EventLogId] on [dbo].[FeedPostEvents]([EventLogId] asc)
include ([Id],[EventDate],[SolutionName],[Comment])

if exists(select 1 from sys.indexes where name='IX_SubmitEvents_EventLogId' and object_id=object_id('SubmitEvents'))
	drop index [IX_SubmitEvents_EventLogId] on [dbo].[SubmitEvents]

create nonclustered index [IX_SubmitEvents_EventLogId] on [dbo].[SubmitEvents]([EventLogId] asc)
include ([Id],[EventDate],[SolutionName],[AssignmentId])






