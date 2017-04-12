-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- The view includes all feed activity types
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create view [dbo].[EventView] as

	select a.Id, a.EventLogId
	from [dbo].[AskForHelpEvents] a

	union all
	select a.Id, a.EventLogId
	from [dbo].[BuildEvents] a
	
	--union all
	--select a.Id, a.EventLogId
	--from [dbo].[CutCopyPasteEvents] a

	--union all
	--select a.Id, a.EventLogId
	--from [dbo].[DebugEvents] a

	--union all
	--select a.Id, a.EventLogId
	--from [dbo].[EditorActivityEvents] a

	union all
	select a.Id, a.EventLogId
	from [dbo].[ExceptionEvents] a

	union all
	select a.Id, a.EventLogId
	from [dbo].[FeedPostEvents] a

	union all
	select a.Id, a.EventLogId
	from [dbo].[HelpfulMarkGivenEvents] a

	union all
	select a.Id, a.EventLogId
	from [dbo].[LogCommentEvents] a

	--union all
	--select a.Id, a.EventLogId
	--from [dbo].[SaveEvents] a

	union all
	select a.Id, a.EventLogId
	from [dbo].[SubmitEvents] a

go