-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- Data clean up: remove EventLogs records that don't have any records in event tables
--                remvoe event table records that don't have matching EventLogs records
--				  RUN THIS BEFORE INDEXES ARE CREATED!!!
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
declare @sqlC nvarchar(max), @eventTypeC varchar(50)

declare eventTypeCursor cursor
for
select distinct LogType from [dbo].[EventLogs]

open eventTypeCursor
fetch next from eventTypeCursor into @eventTypeC
while (@@fetch_status <> -1)
	begin

		set @sqlC=N'delete from [dbo].[EventLogs] where [LogType]=''' + @eventTypeC + ''' and [Id] not in (select [EventLogId] from [dbo].[' + @eventTypeC + 's])'
		exec sp_executesql @sqlC

		set @sqlC=N'delete from [dbo].[' + @eventTypeC + 's] where EventLogId not in (select [Id] from [dbo].[EventLogs])'
		exec sp_executesql @sqlC

		fetch next from eventTypeCursor into @eventTypeC

	end
close eventTypeCursor
deallocate eventTypeCursor

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- Create event log cover indexes on event tables (this whole set runs 7:45 on my localhost)
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
--create nonclustered index [IX_ActionRequestLogs_SenderId] on [dbo].[ActionRequestLogs]([CreatorId] asc, [AccessDate] asc)
--include(Id, ActionName, ActionParameters, ControllerName, IpAddress)

create nonclustered index [IX_DateRecieved_SenderId] on [dbo].[EventLogs]([SenderId] asc, [DateReceived] asc)
include(Id, LogType)

create nonclustered index [IX_AskForHelpEvents_EventLogId] on [dbo].[AskForHelpEvents]([EventLogId] asc)
include(Id, EventDate, Code, SolutionName, UserComment)

create nonclustered index [IX_BuildEvents_EventLogId] on [dbo].[BuildEvents]([EventLogId] asc)
include(Id, EventDate, SolutionName)

create nonclustered index [IX_BuildErrors_LogId] on [dbo].[BuildErrors]([LogId] asc)

create nonclustered index [IX_CutCopyPasteEvents_EventLogId] on [dbo].[CutCopyPasteEvents]([EventLogId] asc)
include(Id, EventDate, Content, DocumentName, EventAction, SolutionName)

create nonclustered index [IX_DebugEvents_EventLogId] on [dbo].[DebugEvents]([EventLogId] asc)
include(Id, EventDate, DebugOutput, DocumentName, ExecutionAction, LineNumber, SolutionName)

create nonclustered index [IX_EditorActivityEvents_EventLogId] on [dbo].[EditorActivityEvents]([EventLogId] asc)
include(Id, EventDate, SolutionName)

create nonclustered index [IX_ExceptionEvents_EventLogId] on [dbo].[ExceptionEvents]([EventLogId] asc)
include(Id, EventDate, DocumentName, ExceptionAction, ExceptionCode, ExceptionDescription, ExceptionName, ExceptionType, LineContent, LineNumber, SolutionName)

create nonclustered index [IX_FeedPostEvents_EventLogId] on [dbo].[FeedPostEvents]([EventLogId] asc)
include(Id, EventDate, Comment, SolutionName)

create nonclustered index [IX_HelpfulMarkGivenEvents_EventLogId] on [dbo].[HelpfulMarkGivenEvents]([EventLogId] asc)
include(Id, EventDate, LogCommentEventId, SolutionName)

create nonclustered index [IX_HelpfulMarkGivenEvents_LogCommentEventId] on [dbo].[HelpfulMarkGivenEvents]([LogCommentEventId] asc)

create nonclustered index [IX_LogCommentEvents_EventLogId] on [dbo].[LogCommentEvents]([EventLogId] asc)
include(Id, EventDate, Content, SolutionName, SourceEventLogId)

create nonclustered index [IX_LogCommentEvents_EventLogIdSourceId] on [dbo].[LogCommentEvents]([EventLogId] asc)
include(SourceEventLogId)

create nonclustered index [IX_LogCommentEvents_SourceEventLogIdAll] on [dbo].[LogCommentEvents]([SourceEventLogId] asc)

create nonclustered index [IX_LogCommentEvents_Id] on [dbo].[LogCommentEvents]([Id] asc)
include(EventDate, Content, SourceEventLogId)

create nonclustered index [IX_SaveEvents_EventLogId] on [dbo].[SaveEvents]([EventLogId] asc)
include(Id, EventDate, DocumentId, SolutionName)

create nonclustered index [IX_SubmitEvents_EventLogId] on [dbo].[SubmitEvents]([EventLogId] asc)
include(Id, EventDate, AssignmentId, SolutionName)

