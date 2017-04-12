-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetBuildDocUtilUsers]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetBuildDocUtilUsers]
as
begin

	set nocount on;

	select distinct l.SenderId
	from [dbo].[EventLogs] l with(nolock)
	inner join [dbo].[BuildEvents] b with(nolock) on b.EventLogId=l.Id
	inner join [dbo].[BuildDocuments] d with(nolock) on d.BuildId=b.Id and d.UpdatedBy is null

end

