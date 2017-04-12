-------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------
-- The view includes all event types with original EventDate and event time at minute level (EventTimeMin)
-------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------
create view [dbo].[EventTimeView] as

	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[AskForHelpEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[BuildEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[CutCopyPasteEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[DebugEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[EditorActivityEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[ExceptionEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[FeedPostEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[HelpfulMarkGivenEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[LogCommentEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[SaveEvents]
	union all
	select EventLogId, EventDate, SolutionName, EventTimeMin=convert(varchar(25), dateadd(minute, datediff(minute, 0, EventDate), 0), 120)
	from [dbo].[SubmitEvents]

