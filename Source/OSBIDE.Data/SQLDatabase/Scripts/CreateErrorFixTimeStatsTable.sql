-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- create ErrorFixTimeStatistics table
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
if not exists (
	select 1 from sys.tables t
	inner join sys.schemas s on s.schema_id=t.schema_id
	where t.name='ErrorFixTimeStatistics' and s.name='dbo')

	begin

	create table [dbo].[ErrorFixTimeStatistics]
	(
		[ErrorTypeId] int not null primary key,
		[Mean] decimal(38,6) not null,
		[StandardDeviation] decimal(38,6) not null,
		[PopulationStandardDeviation] decimal(38,6) not null,
		[CreatedOn] datetime not null,
		[CreatedBy] int not null,
		[UpdatedOn] datetime null,
		[UpdatedBy] int null
	)

	end

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- populate stats
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
declare @now datetime=getDate()
declare @userId int=0
insert into [dbo].[ErrorFixTimeStatistics]
select [ErrorTypeId]=stats.BuildErrorTypeId, [Mean]=avg(timeToFix), [StandardDeviation]=stdevp(TimeToFix), [PopulationStandardDeviation]=stdevp(TimeToFix), CreatedOn=@now, CreatedBy=@userId, UpdatedOn=@now, UpdatedBy=@userId
from
(
	select l.Id, b.EventDate, l.SenderId, e.BuildErrorTypeId, TimeToFix=case when datediff(d, b.EventDate, min(fixes.EventDate)) < 30 then cast(datediff(s, b.EventDate, min(fixes.EventDate)) as decimal(38,0)) else 2592000 /*30 days*/ end
	from [dbo].[EventLogs] l
	inner join [dbo].[BuildEvents] b on b.EventLogId=l.Id
	inner join [dbo].[BuildErrors] e on e.LogId=l.Id
	inner join
	(
		select b.EventDate, l.SenderId, BuildErrorTypeId=isNull(e.BuildErrorTypeId, 0)
		from [dbo].[EventLogs] l
		inner join [dbo].[BuildEvents] b on b.EventLogId=l.Id
		left join [dbo].[BuildErrors] e on e.LogId=l.Id
	)
	fixes on fixes.SenderId=l.SenderId and fixes.BuildErrorTypeId<>e.BuildErrorTypeId and fixes.EventDate>l.DateReceived
	group by l.Id, b.EventDate, l.SenderId, e.BuildErrorTypeId
)
stats
group by stats.BuildErrorTypeId
