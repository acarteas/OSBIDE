-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetStateMachineEvents]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
alter procedure [dbo].[GetStateMachineEvents]

	 @dateFrom DateTime
	,@dateTo DateTime
	,@studentIds varchar(max)

as
begin

	set nocount on;

	declare @minDate datetime='1-2-2012'

	declare @users table(UserId int)
	insert into @users select UserId=cast(items as int) from dbo.Split(@studentIds, ',')

	select distinct l.SenderId, v.EventDate, v.SolutionName, l.LogType, BuildErrorLogId=be.LogId, ExecutionActionId=d.ExecutionAction, eu.FirstName, eu.LastName, eu.InstitutionId
	, MarkerType=case when l.LogType='AskForHelpEvent' or p.Comment like '%?%' then 'QP'
					  when p.Comment not like '%?%' then 'NP'

					  when ol.SenderId<>l.SenderId and p.Comment like '%?%' then 'RQ'
					  when ol.SenderId<>l.SenderId and p.Comment not like '%?%' then 'RN'

					  when ol.SenderId=l.SenderId and p.Comment like '%?%' then 'QF'
					  when ol.SenderId=l.SenderId and p.Comment not like '%?%' then 'NF'

					  when sl.SenderId<>l.SenderId and sp.Comment like '%?%' then 'QR'
					  when sl.SenderId<>l.SenderId and sp.Comment not like '%?%' then 'NR'

					  when sl.SenderId=l.SenderId and sp.Comment like '%?%' then 'QF'
					  when sl.SenderId=l.SenderId and sp.Comment not like '%?%' then 'NF'
				 end
	from @users u
	inner join [dbo].[EventLogs] l on l.SenderId=u.UserId
	inner join [dbo].[EventTimeView] v on v.EventLogId=l.Id
	inner join [dbo].[OsbideUsers] eu on eu.Id=l.SenderId
	left join [dbo].[BuildErrors] be on be.LogId=l.Id
	left join [dbo].[DebugEvents] d on d.EventLogId=l.Id
	-- subject user's posts with answers
	left join ([dbo].[FeedPostEvents] p
				inner join [dbo].[EventLogs] ol on ol.Id=p.EventLogId
				left join [dbo].[LogCommentEvents] oc on oc.EventLogId=ol.Id) on p.EventLogId=l.Id
	-- subject user's comments on posts
	left join ([dbo].[LogCommentEvents] sc
				inner join [dbo].[EventLogs] sl on sl.Id=sc.SourceEventLogId
				inner join [dbo].[FeedPostEvents] sp on sp.EventLogId=sl.Id) on sc.EventLogId=l.Id
	-- subject user's comments on ask for help
	left join ([dbo].[LogCommentEvents] hc
				inner join [dbo].[EventLogs] hl on hl.Id=hc.SourceEventLogId
				inner join [dbo].[AskForHelpEvents] hp on hp.EventLogId=hl.Id) on hc.EventLogId=l.Id
	where v.EventDate between @dateFrom and @dateTo
		or @dateFrom<@minDate and v.EventDate<=@dateTo
		or @dateTo<@minDate and v.EventDate>=@dateFrom
		or @dateFrom<@minDate and @dateTo<@minDate

	union all
	select SenderId=e.UserId, EventDate=e.AccessDate,
	SolutionName=null, LogType='PassiveSocialEvent', BuildErrorLogId=null,ExecutionActionId=null,
	u.FirstName, u.LastName, u.InstitutionId, MarkerType=e.EventCode
	from @users uu
	inner join [dbo].[PassiveSocialEvents] e on e.UserId=uu.UserId
	inner join [dbo].[OsbideUsers] u on u.Id=e.UserId
	where e.AccessDate between @dateFrom and @dateTo
		or @dateFrom<@minDate and e.AccessDate<=@dateTo
		or @dateTo<@minDate and e.AccessDate>=@dateFrom
		or @dateFrom<@minDate and @dateTo<@minDate

	--order by l.SenderId, v.EventDate

end


/*
exec GetStateMachineEvents @dateFrom='1-1-2010', @dateTo='1-1-2015', @studentIds='68,95,157,264'
exec [dbo].[GetStateMachineEvents] @dateFrom='2014-01-17 12:00:00',@dateTo='2014-01-18 12:00:00',@studentIds='68,157'
*/



