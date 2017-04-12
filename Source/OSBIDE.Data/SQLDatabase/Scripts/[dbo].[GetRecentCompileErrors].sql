-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetRecentCompileErrors]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetRecentCompileErrors]

	 @SenderId int
	,@Timeframe datetime

as
begin

	set nocount on;

	select ei.Id, ei.[Column], ei.[Description], ei.[File], ei.Line, ei.Project
	from [dbo].[EventLogs] s
	inner join [dbo].[BuildEvents] e on e.EventLogId=s.Id
	inner join [dbo].[BuildErrors] be on be.LogId=s.Id -- starts with "error" constraint
	inner join [dbo].[BuildEventErrorListItems] el on el.BuildEventId=e.EventLogId
	inner join [dbo].[ErrorListItems] ei on ei.Id=el.ErrorListItemId
	where s.SenderId=@SenderId and s.DateReceived > @Timeframe

end


--exec [dbo].[GetRecentCompileErrors] @senderId=1, @timeframe='2010-1-1'

