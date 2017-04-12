-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetPassiveSocialEvents]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetPassiveSocialEvents]

	 @skip int
	,@take int

as
begin

	set nocount on;

	
/*
	declare @logIdOffset int=len('singleLogId=')
	select id, UserId=CreatorId, EventDate=AccessDate,
	EventLogId=cast(substring(ActionParameters, @logIdOffset+1, len(ActionParameters)-@logIdOffset-3) as int)
	from ActionRequestLogs
	where ActionParameters <> 'singleLogId=[null]|||' and ActionParameters like 'singleLogId=%'
	order by Id
	offset @skip rows
	fetch next @take rows only
*/

end

