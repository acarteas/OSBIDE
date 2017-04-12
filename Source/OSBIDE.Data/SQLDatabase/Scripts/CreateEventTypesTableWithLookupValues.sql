
create table [dbo].[EventTypes]
(
	[EventTypeId] [int] identity(1,1) not null,
	[EventTypeName] [varchar](50) not null,
	[IsSocialEvent] [bit] not null,
	[IsIDEEvent] [bit] not null,
	[IsFeedEvent] [bit] not null,
	[IsEditEvent] [bit] null,
	[EventTypeCategoryId] [int] null,
	constraint [PK_dbo.EventType] primary key clustered([EventTypeId] asc)
)

insert into [dbo].[EventTypes] (EventTypeName, IsSocialEvent, IsIDEEvent, IsFeedEvent) values
 ('AskForHelpEvent', 1, 0, 1)
,('BuildEvent', 0, 1, 1)
,('CutCopyPasteEvent', 0, 0, 0)
,('DebugEvent', 0, 0, 0)
,('EditorActivityEvent', 0, 0, 0)
,('ExceptionEvent', 0, 1, 1)
,('FeedPostEvent', 1, 0, 1)
,('HelpfulMarkGivenEvent', 1, 0, 1)
,('LogCommentEvent', 1, 0, 1)
,('SaveEvent', 0, 0, 0)
,('SubmitEvent', 1, 0, 1)
go

