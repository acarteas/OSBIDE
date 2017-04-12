-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetErrorQuotientSessionData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetErrorQuotientSessionData]

	 @dateFrom DateTime
	,@dateTo DateTime
	,@userIds nvarchar(max)
as
begin

	set nocount on;

	declare @users table(userId int)
	insert into @users select userId=cast(items as int) from [dbo].[Split](@userIds, ',')

	declare @minDate datetime='1-1-2010'
	select distinct BuildId=b.Id, LogId=b.EventLogId, b.EventDate, e.SenderId
	from @users u
	inner join [dbo].[EventLogs] e with(nolock) on e.SenderId=u.userId
	inner join [dbo].[BuildEvents] b with(nolock) on b.EventLogId=e.Id
	where (b.EventDate between @dateFrom and @dateTo
		or @dateFrom<@minDate and b.EventDate<=@dateTo
		or @dateTo<@minDate and b.EventDate>=@dateFrom
		or @dateFrom<@minDate and @dateTo<@minDate)

end



/*

exec [dbo].[GetErrorQuotientSessionData] @dateFrom='2000-01-01',@dateTo='2014-05-03',@userIds='150,54,151'

*/

