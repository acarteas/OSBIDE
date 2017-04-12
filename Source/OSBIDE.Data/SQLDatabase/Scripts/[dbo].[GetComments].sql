-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetActivityFeeds]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetComments]

	 @EventLogIds nvarchar(max)
	,@CurrentUserId int
as
begin

	set nocount on;

	--declare @EventLogIds nvarchar(max)='458485,458484,458483,458482,458480,458479,458478,458477,458476,458475,457745,456755,456524,456193,456041,456025,455995,455984,455815,455803,455798,455785,455765,455760,455750,455738,455729,455292,455283,455271,455090,454969,454955,454945,454926,454903,454829,454788,454678'
	--declare @CurrentUserId int=1

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Subject events
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	declare @events table(EventLogId int)
	insert into @events select EventLogId=cast(items as int) from [dbo].[Split](@EventLogIds, ',')

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Subject user's helpful comment marks
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	declare @helpfulMarks table (LogCommentEventId int, HelpfulMarkCounts int, HelpfulMarkSenderId int)
	insert into @helpfulMarks
	select h.LogCommentEventId, HelpfulMarkCounts=count(h.Id), HelpfulMarkSenderId=max(isnull(hl.SenderId,0))
	from [dbo].[HelpfulMarkGivenEvents] h with (nolock)
	inner join [dbo].[EventLogs] hl with (nolock) on hl.Id=h.EventLogId and hl.SenderId=@CurrentUserId
	group by h.LogCommentEventId


	declare @returnTable table
	(
		 CommentId int
		,OriginalId int
		,ActualId int
		,CourseNumber nvarchar(max)
		,CoursePrefix nvarchar(max)
		,Content nvarchar(max)
		,LastName nvarchar(max)
		,FirstName nvarchar(max)
		,SenderId int
		,EventDate datetime
		,HelpfulMarkCounts int
		,IsHelpfulMarkSender bit
	)

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Select comment events whose source events are in the @EventLogIds
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	insert into @returnTable
	select CommentId=c.Id
		  ,OriginalId=e.EventLogId
		  ,ActualId=c.SourceEventLogId
		  ,cs.CourseNumber
		  ,CoursePrefix=cs.Prefix
		  ,c.Content
		  ,u.LastName
		  ,u.FirstName
		  ,l.SenderId
		  ,c.EventDate
		  ,HelpfulMarkCounts=isnull(hm.HelpfulMarkCounts,0)
		  ,IsHelpfulMarkSender=case when hm.HelpfulMarkSenderId > 0 then cast(1 as bit) else cast(0 as bit) end
	from @events e
	inner join [dbo].[LogCommentEvents] c with (nolock) on c.SourceEventLogId=e.EventLogId
	inner join [dbo].[EventLogs] l with (nolock) on l.Id=c.EventLogId
	inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=l.SenderId
	inner join [dbo].[Courses] cs with (nolock) on cs.Id=u.DefaultCourseId
	left join @helpfulMarks hm on hm.LogCommentEventId=c.Id

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Select source event comments of each comment event included in the @EventLogIds
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	insert into @returnTable
	select CommentId=cb.Id
		  ,OriginalId=e.EventLogId
		  ,ActualId=cb.SourceEventLogId
		  ,cs.CourseNumber
		  ,CoursePrefix=cs.Prefix
		  ,cb.Content
		  ,u.LastName
		  ,u.FirstName
		  ,l.SenderId
		  ,cb.EventDate
		  ,HelpfulMarkCounts=isnull(hm.HelpfulMarkCounts,0)
		  ,IsHelpfulMarkSender=case when hm.HelpfulMarkSenderId > 0 then cast(1 as bit) else cast(0 as bit) end
	from @events e
	inner join [dbo].[LogCommentEvents] c with (nolock) on c.EventLogId=e.EventLogId
	inner join [dbo].[LogCommentEvents] cb with (nolock) on cb.SourceEventLogId=c.SourceEventLogId -- sibling comments
	inner join [dbo].[EventLogs] l with (nolock) on l.Id=cb.EventLogId
	inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=l.SenderId
	inner join [dbo].[Courses] cs with (nolock) on cs.Id=u.DefaultCourseId
	left join @helpfulMarks hm on hm.LogCommentEventId=cb.Id

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Select source events of the helpful mark events included in the @EventLogIds
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	insert into @returnTable
	select CommentId=cb.Id
		  ,OriginalId=e.EventLogId
		  ,ActualId=cb.SourceEventLogId
		  ,cs.CourseNumber
		  ,CoursePrefix=cs.Prefix
		  ,cb.Content
		  ,u.LastName
		  ,u.FirstName
		  ,l.SenderId
		  ,cb.EventDate
		  ,HelpfulMarkCounts=isnull(hm.HelpfulMarkCounts,0)
		  ,IsHelpfulMarkSender=case when hm.HelpfulMarkSenderId > 0 then cast(1 as bit) else cast(0 as bit) end
	from @events e
	inner join [dbo].[HelpfulMarkGivenEvents] h with (nolock) on h.EventLogId=e.EventLogId
	inner join [dbo].[LogCommentEvents] c with (nolock) on c.Id=h.LogCommentEventId
	inner join [dbo].[LogCommentEvents] cb with (nolock) on cb.SourceEventLogId=c.SourceEventLogId -- sibling comments
	inner join [dbo].[EventLogs] l with (nolock) on l.Id=cb.EventLogId
	inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=l.SenderId
	inner join [dbo].[Courses] cs with (nolock) on cs.Id=u.DefaultCourseId
	left join @helpfulMarks	hm on hm.LogCommentEventId=cb.Id

	select distinct CommentId
		  ,OriginalId
		  ,ActualId
		  ,CourseNumber
		  ,CoursePrefix
		  ,Content
		  ,LastName
		  ,FirstName
		  ,SenderId
		  ,EventDate
		  ,HelpfulMarkCounts
		  ,IsHelpfulMarkSender
	from @returnTable

end




