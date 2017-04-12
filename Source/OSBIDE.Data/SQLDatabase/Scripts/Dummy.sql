-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- These sprocs can help to generate the _result mappers, only needed in dev environment
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetAskForHelpEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.Code, a.SolutionName, a.UserComment
	from [dbo].[AskForHelpEvents] a
end
go

create procedure [dbo].[GetBuildEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.SolutionName
	from [dbo].[BuildEvents] a
end
go

create procedure [dbo].[GetCutCopyPasteEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.Content, a.DocumentName, a.EventAction, a.SolutionName
	from [dbo].[CutCopyPasteEvents] a
end
go

create procedure [dbo].[GetDebugEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.DebugOutput, a.DocumentName, a.ExecutionAction, a.LineNumber, a.SolutionName
	from [dbo].[DebugEvents] a
end
go

create procedure [dbo].[GetEditorActivityEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.SolutionName
	from [dbo].[EditorActivityEvents] a
end
go

create procedure [dbo].[GetExceptionEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.DocumentName, a.ExceptionAction, a.ExceptionCode, a.ExceptionDescription, a.ExceptionName, a.ExceptionType, a.LineContent, a.LineNumber, a.SolutionName
	from [dbo].[ExceptionEvents] a
end
go

create procedure [dbo].[GetFeedPostEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.Comment, a.SolutionName
	from [dbo].[FeedPostEvents] a
end
go

create procedure [dbo].[GetLogCommentEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.Content, a.SolutionName, a.SourceEventLogId
	from [dbo].[LogCommentEvents] a
end
go

create procedure [dbo].[GetHelpfulMarkGivenEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.LogCommentEventId, a.SolutionName
	from [dbo].[HelpfulMarkGivenEvents] a
end
go

create procedure [dbo].[GetSaveEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.DocumentId, a.SolutionName
	from [dbo].[SaveEvents] a
end
go

create procedure [dbo].[GetSubmitEvents]

	 @SenderId int
	,@DateReceived datetime
	,@EventTypes nvarchar(100)
as
begin

	set nocount on;
	select top 10 a.Id, a.EventLogId, a.EventDate, a.AssignmentId, a.SolutionData, a.SolutionName
	from [dbo].[SubmitEvents] a
end
go

create procedure [dbo].[GetEventLogs]

	 @DateReceivedMin datetime
	,@DateReceivedMax datetime
as
begin

	set nocount on;
	select top 10 Id, LogType, DateReceived, SenderId from EventLogs
end
go

create procedure [dbo].[GetOsbideUsers]

	 @DateReceivedMin datetime
	,@DateReceivedMax datetime
as
begin

	set nocount on;
	select top 10 a.Id
		 , a.Email
		 , a.FirstName
		 , a.LastName
		 , a.SchoolId
		 , a.InstitutionId
		 , a.ReceiveEmailOnNewAskForHelp
		 , a.ReceiveEmailOnNewFeedPost
		 , a.ReceiveNotificationEmails
		 , a.DefaultCourseId
		 , a.LastVsActivity
		 , DefaultCourseName=c.Prefix + ' ' + c.CourseNumber
	from [dbo].[OsbideUsers] a
	inner join [dbo].[Courses] c on c.Id=a.DefaultCourseId
end
go

create procedure [dbo].[GetLogComments]

	 @DateReceivedMin datetime
	,@DateReceivedMax datetime
as
begin

	set nocount on;
	select top 10 a.Id
		 , a.EventLogId
		 , a.SourceEventLogId
		 , a.EventDate
		 , a.SolutionName
		 , a.Content
		 , c.LogType
		 , c.SenderId
		 , c.DateReceived
	from [dbo].[LogCommentEvents] a
	inner join [dbo].[EventLogs] c on c.Id=a.SourceEventLogId
end
go

create procedure [dbo].[GetSubscriptions]

	 @DateReceivedMin datetime
	,@DateReceivedMax datetime
as
begin

	set nocount on;
	select top 10 a.UserId
		 , a.LogId
	from [dbo].[EventLogSubscriptions] a
end
go

create procedure [dbo].[GetProfileImage]

	 @DateReceivedMin datetime
	,@DateReceivedMax datetime
as
begin

	set nocount on;
	select top 10 g.UserID, g.Picture
	from [dbo].[ProfileImages] g
end
go

--select top(400) s.Id, s.LogType, s.DateReceived, s.SenderId 
--from [dbo].[EventLogs] s 
--where s.SenderId>0 and s.DateReceived > 'Jan  1 2010 12:00AM' and s.DateReceived < 'Mar  3 2014 12:00AM' 
--order by s.DateReceived desc


