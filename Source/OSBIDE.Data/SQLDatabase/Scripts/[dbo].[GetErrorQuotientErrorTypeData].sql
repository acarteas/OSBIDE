-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetErrorQuotientErrorTypeData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetErrorQuotientErrorTypeData]

	@buildIds nvarchar(max)
as
begin

	set nocount on;

	declare @logs table(EventLogId int)
	insert into @logs select buildId=cast(items as int) from [dbo].[Split](@buildIds, ',')

	select distinct be.LogId, ErrorTypeId=be.BuildErrorTypeId
	from @logs a
	inner join [dbo].[BuildErrors] be with(nolock) on be.LogId=a.EventLogId

end



/*

exec [dbo].[GetErrorQuotientSessionData] @dateFrom='2000-01-01',@dateTo='2014-05-03',@userIds='150,54,151'

*/

