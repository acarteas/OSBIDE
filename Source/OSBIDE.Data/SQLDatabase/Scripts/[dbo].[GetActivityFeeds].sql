-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetActivityFeeds]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
alter procedure [dbo].[GetActivityFeeds]

	 @DateReceivedMin datetime='01-01-2010'
	,@DateReceivedMax datetime
	,@EventLogIds nvarchar(max)=''
	,@EventTypes nvarchar(max)=''
	,@CourseId int=0
	,@RoleId int=99
	,@CommentFilter nvarchar(max)=''
	,@SenderIds nvarchar(max)=''
	,@MinLogId int=0
	,@MaxLogId int=0
	,@OffsetN int=0
	,@TopN int=20
as
begin

	set nocount on;

	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- Subject Eventlogs
	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	declare @eventTypesFilter table(EventTypeId int)
	insert into @eventTypesFilter select EventTypeId=cast(items as int) from [dbo].[Split](@EventTypes, ',')
	if not exists(select 1 from @eventTypesFilter) insert into @eventTypesFilter select EventTypeId from [dbo].[EventTypes] where IsFeedEvent=1

	declare @senderIdFilter table(Id int)
	insert into @senderIdFilter select Id=cast(items as int) from [dbo].[Split](@SenderIds, ',')

	declare @eventLogIdFilter table(Id int)
	insert into @eventLogIdFilter select Id=cast(items as int) from [dbo].[Split](@EventLogIds, ',')

	create table #events (Id int, LogType varchar(100), DateReceived datetime, SenderId int, IsResult bit)

	if len(@CommentFilter) > 0
	begin
		insert into #events
		select top(@TopN) s.Id, s.LogType, s.DateReceived, s.SenderId, 1
		from [dbo].[EventLogs] s with (nolock)
		inner join @eventTypesFilter ef on ef.EventTypeId=s.EventTypeId
		inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType>=@RoleId or @RoleId=99)
		inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId and (u.DefaultCourseId=@CourseId or @CourseId=0)
		left join (select buildErrors=count(BuildErrorTypeId), LogId from [dbo].[BuildErrors] with (nolock) group by LogId) be on s.Id=be.LogId
		left join [dbo].[FeedPostEvents] fp on fp.EventLogId=s.Id and fp.Comment like @CommentFilter
		left join ([dbo].[LogCommentEvents] lc
						inner join [dbo].[FeedPostEvents] lcfp on lcfp.EventLogId=lc.SourceEventLogId and lcfp.Comment like @CommentFilter
				  ) on lc.EventLogId=s.Id
		left join ([dbo].[HelpfulMarkGivenEvents] hm
					inner join [dbo].[LogCommentEvents] hmlc on hmlc.Id=hm.LogCommentEventId
					inner join [dbo].[FeedPostEvents] hmlcfp on hmlcfp.EventLogId=hmlc.SourceEventLogId and hmlcfp.Comment like @CommentFilter
				  ) on hm.EventLogId=s.Id
		where (ef.EventTypeId=7 and fp.Id>0 or ef.EventTypeId=9 and lcfp.Id>0 or ef.EventTypeId=8 and hmlcfp.Id>0 or ef.EventTypeId not in (7,8,9))
				and (ef.EventTypeId=2 and be.buildErrors>0 or ef.EventTypeId<>2)
				and (len(@SenderIds)=0 or s.SenderId in (select Id from @senderIdFilter))
				and (len(@EventLogIds)=0 or s.Id in (select Id from @eventLogIdFilter))
				and (@MinLogId=-1 or s.Id>@MinLogId)
				and (@MaxLogId=-1 or s.Id<@MaxLogId)
				and s.DateReceived between @DateReceivedMin and @DateReceivedMax
		order by s.DateReceived desc
	end
	else
	begin
		insert into #events
		select top(@TopN) s.Id, s.LogType, s.DateReceived, s.SenderId, 1
		from [dbo].[EventLogs] s with (nolock)
		inner join @eventTypesFilter ef on ef.EventTypeId=s.EventTypeId
		inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType>=@RoleId or @RoleId=99)
		inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId and (u.DefaultCourseId=@CourseId or @CourseId=0)
		left join (select buildErrors=count(BuildErrorTypeId), LogId from [dbo].[BuildErrors] with (nolock) group by LogId) be on s.Id=be.LogId and (ef.EventTypeId=2 and be.buildErrors>0 or ef.EventTypeId<>2)
		where s.DateReceived between @DateReceivedMin and @DateReceivedMax
				and (len(@SenderIds)=0 or s.SenderId in (select Id from @senderIdFilter))
				and (len(@EventLogIds)=0 or s.Id in (select Id from @eventLogIdFilter))
				and (@MinLogId=-1 or s.Id>@MinLogId)
				and (@MaxLogId=-1 or s.Id<@MaxLogId)				
		order by s.DateReceived desc
	end

	-- for comment events add their sources
	insert into #events
	select s.Id, s.LogType, s.DateReceived, s.SenderId, 0
	from [dbo].[EventLogs] s with (nolock)
	inner join [dbo].[LogCommentEvents] cs with (nolock) on cs.SourceEventLogId=s.Id
	inner join #events e on e.Id=cs.EventLogId
	inner join [dbo].[FeedPostEvents] fp with (nolock) on fp.EventLogId=cs.SourceEventLogId and (len(@CommentFilter)=0 or fp.Comment like @CommentFilter)
	--inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType=@RoleId or @RoleId=-1)
	--inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId
	where s.Id not in (select id from #events)

	-- for helpful mark events add their comments and comments sources
	insert into #events
	select s.Id, s.LogType, s.DateReceived, s.SenderId, 0
	from #events e
	inner join [dbo].[HelpfulMarkGivenEvents] hm with (nolock) on hm.EventLogId=e.Id
	inner join [dbo].[LogCommentEvents] cs with (nolock) on cs.Id=hm.LogCommentEventId
	inner join [dbo].[EventLogs] s with (nolock) on (s.Id=cs.EventLogId or s.Id=cs.SourceEventLogId)
	--inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType=@RoleId or @RoleId=-1)
	--inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId
	where s.Id not in (select id from #events)

	-- for post events add their comments
	insert into #events
	select s.Id, s.LogType, s.DateReceived, s.SenderId, 0
	from #events e with (nolock)
	inner join [dbo].[LogCommentEvents] cs with (nolock) on cs.SourceEventLogId=e.Id
	inner join #events s on s.Id=cs.EventLogId
	where s.Id not in (select id from #events)

	create clustered index IX_Temp_Events on #events(Id)

	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- Top level results
	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- EventLogs 
	select Id, LogType, DateReceived, SenderId, IsResult from #events

	-- Event and Comment Users 
	select distinct a.Id
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
		 , DefaultCourseNumber=c.CourseNumber
		 , DefaultCourseNamePrefix=c.Prefix
	from [dbo].[OsbideUsers] a with (nolock)
	inner join [dbo].[Courses] c with (nolock) on c.Id=a.DefaultCourseId
	inner join #events b on b.SenderId=a.Id

	-- ActivityComments 
	select a.Id
		 , a.EventLogId
		 , a.SourceEventLogId
		 , a.EventDate
		 , a.SolutionName
		 , a.Content
		 , c.LogType
		 , c.SenderId
		 , c.DateReceived
	from [dbo].[LogCommentEvents] a with (nolock)
	inner join #events b on b.Id=a.SourceEventLogId
	inner join [dbo].[EventLogs] c with (nolock) on c.Id=b.Id
	
	-- UserSubscriptions 
	select a.UserId
		 , a.LogId
	from [dbo].[EventLogSubscriptions] a with (nolock)
	inner join #events b on b.Id=a.LogId

	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- Detailed event types
	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	select a.Id, a.EventLogId, a.EventDate, a.Code, a.SolutionName, a.UserComment
	from [dbo].[AskForHelpEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.SolutionName, CriticalErrorName=[Description]
	from [dbo].[BuildEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId
	left join
	(
		select be.[BuildEventId], e.[Description]
		from [dbo].[BuildEventErrorListItems] be
		inner join [dbo].[ErrorListItems] e on e.[Id]=be.[ErrorListItemId]
		inner join [dbo].[ErrorTypes] t on 'error ' + t.Name + ':'=substring(e.[Description], 1, charindex(':', e.[Description]))
	)
	builderror on builderror.[BuildEventId]=a.Id

	select a.Id, a.EventLogId, a.EventDate, a.DocumentName, a.ExceptionAction, a.ExceptionCode, a.ExceptionDescription, a.ExceptionName, a.ExceptionType, a.LineContent, a.LineNumber, a.SolutionName
	from [dbo].[ExceptionEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.Comment, a.SolutionName
	from [dbo].[FeedPostEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId
	where a.Comment like @CommentFilter or len(@CommentFilter)=0

	select a.Id, a.EventLogId, a.EventDate, a.LogCommentEventId, a.SolutionName
	from [dbo].[HelpfulMarkGivenEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.Content, a.SolutionName, a.SourceEventLogId
	from [dbo].[LogCommentEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId
	inner join [dbo].[FeedPostEvents] fp with (nolock) on fp.EventLogId=a.SourceEventLogId
	where len(@CommentFilter)=0 or fp.Comment like @CommentFilter

	select a.Id, a.EventLogId, a.EventDate, a.AssignmentId, a.SolutionName
	from [dbo].[SubmitEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

end


/*
exec [dbo].[GetActivityFeeds] @DateReceivedMax='9-6-2014'

select * from eventTypes

declare @DateReceivedMin datetime='2010-01-01', @DateReceivedMax datetime='2014-09-09'
declare @EventLogIds varchar(200)=N''
declare @EventTypes varchar(200)=N'2,6,7,1,9,8,11'
declare @CourseId int=0, @RoleId int=0, @CommentFilter varchar(200)=N'', @SenderIds varchar(200)=N''
declare @MinLogId int=-1, @MaxLogId int=-1, @OffsetN int=0, @TopN int=40

declare @eventTypesFilter table(EventTypeId int)
insert into @eventTypesFilter select EventTypeId=cast(items as int) from [dbo].[Split](@EventTypes, ',')
if not exists(select 1 from @eventTypesFilter) insert into @eventTypesFilter select EventTypeId from [dbo].[EventTypes] where IsFeedEvent=1

declare @senderIdFilter table(Id int)
insert into @senderIdFilter select Id=cast(items as int) from [dbo].[Split](@SenderIds, ',')

declare @eventLogIdFilter table(Id int)
insert into @eventLogIdFilter select Id=cast(items as int) from [dbo].[Split](@EventLogIds, ',')


select top(@TopN) s.Id, s.LogType, s.DateReceived, s.SenderId, 1
from [dbo].[EventLogs] s with (nolock)
inner join @eventTypesFilter ef on ef.EventTypeId=s.EventTypeId
--inner join [dbo].[EventView] ev with (nolock) on ev.EventLogId=s.Id
--inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType>=@RoleId or @RoleId=99)
--inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId and (u.DefaultCourseId=@CourseId or @CourseId=0)
left join (select buildErrors=count(BuildErrorTypeId), LogId from [dbo].[BuildErrors] with (nolock) group by LogId) be on s.Id=be.LogId
where (ef.EventTypeId=2 and be.buildErrors>0 or ef.EventTypeId<>2)
		and (len(@SenderIds)=0 or s.SenderId in (@SenderIds))
		and (len(@EventLogIds)=0 or s.Id in (@EventLogIds))
		and (@MinLogId=-1 or s.Id>@MinLogId)
		and (@MaxLogId=-1 or s.Id<@MaxLogId)
		and s.DateReceived between @DateReceivedMin and @DateReceivedMax
order by s.DateReceived

*/




