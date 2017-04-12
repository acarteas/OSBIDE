-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetPrecalculatedStateMachineEvents]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetPrecalculatedStateMachineEvents]

	 @dateFrom DateTime
	,@dateTo DateTime
	,@studentIds varchar(max)
as
begin

	set nocount on;

	declare @minDate datetime='1-2-2012'

	declare @users table(UserId int)
	insert into @users select UserId=cast(items as int) from dbo.Split(@studentIds, ',')

	select l.SenderId, l.EventDate, l.SolutionName, l.LogType, l.BuildErrorLogId, l.ExecutionActionId,
	l.FirstName, l.LastName, l.InstitutionId, l.MarkerType
	from @users uu
	inner join [dbo].[ActiveSocialEvents] l on l.SenderId=uu.UserId
	where l.EventDate between @dateFrom and @dateTo
		or @dateFrom<@minDate and l.EventDate<=@dateTo
		or @dateTo<@minDate and l.EventDate>=@dateFrom
		or @dateFrom<@minDate and @dateTo<@minDate

	union all
	select SenderId=e.UserId, EventDate=e.AccessDate,
	SolutionName=null, LogType='PassiveSocialEvent', BuildErrorLogId=null,ExecutionAction='',
	u.FirstName, u.LastName, u.InstitutionId, MarkerType=e.EventCode
	from @users uu
	inner join [dbo].[PassiveSocialEvents] e on e.UserId=uu.UserId
	inner join [dbo].[OsbideUsers] u on u.Id=e.UserId
	where e.AccessDate between @dateFrom and @dateTo
		or @dateFrom<@minDate and e.AccessDate<=@dateTo
		or @dateTo<@minDate and e.AccessDate>=@dateFrom
		or @dateFrom<@minDate and @dateTo<@minDate

end


/*
exec GetPrecalculatedStateMachineEvents @dateFrom='1-1-2010', @dateTo='1-1-2015', @studentIds='68,95,157,264'
exec [dbo].[GetPrecalculatedStateMachineEvents] @dateFrom='2014-01-17 12:00:00',@dateTo='2014-01-18 12:00:00',@studentIds='68,157'
*/



