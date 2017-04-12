-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetWatwinScoringErrorTypeData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetWatwinScoringErrorTypeData]

	@buildIds nvarchar(max)
as
begin

	set nocount on;

	declare @errorFixTimeoutInDays int=14
	declare @errorFixTimeoutInSeconds int=86400*(@errorFixTimeoutInDays+1)
	declare @logs table(EventLogId int)
	insert into @logs select buildId=cast(items as int) from [dbo].[Split](@buildIds, ',')

	select LogId=l.Id, e.BuildErrorTypeId, BuildErrorType=et.Name
	,TimeToFix=case when datediff(d, be.EventDate, min(fixes.EventDate)) < @errorFixTimeoutInDays then cast(datediff(s, be.EventDate, min(fixes.EventDate)) as bigint) else @errorFixTimeoutInSeconds end
	from @logs b
	inner join [dbo].[BuildEvents] be on be.EventLogId=b.EventLogId
	inner join [dbo].[EventLogs] l on l.Id=b.EventLogId
	inner join [dbo].[BuildErrors] e on e.LogId=l.Id
	inner join [dbo].[ErrorTypes] et on et.Id=e.BuildErrorTypeId
	left join
	(
		select be.EventDate, l.SenderId, BuildErrorTypeId=isNull(e.BuildErrorTypeId, 0)
		from @logs b
		inner join [dbo].[BuildEvents] be on be.EventLogId=b.EventLogId
		inner join [dbo].[EventLogs] l on l.Id=b.EventLogId
		left join [dbo].[BuildErrors] e on e.LogId=l.Id
	)
	fixes on fixes.SenderId=l.SenderId and fixes.BuildErrorTypeId<>e.BuildErrorTypeId and fixes.EventDate>be.EventDate
	group by l.Id, be.EventDate, e.BuildErrorTypeId, et.Name

end
