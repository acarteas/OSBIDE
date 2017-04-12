-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetBuildDocUtilBatch]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetBuildDocUtilBatch]

	 @UserIds nvarchar(max)
as
begin

	set nocount on;
	
	declare @Users table(UserId int)
	insert into @Users select UserId=cast(items as int) from [dbo].[Split](@UserIds, ',')

	select l.Id, d.BuildId, d.DocumentId, l.SenderId, b.EventDate, b.SolutionName, c.[FileName], c.Content
	from @Users u
	inner join [dbo].[EventLogs] l with(nolock) on l.SenderId=u.UserId
	inner join [dbo].[BuildEvents] b with(nolock) on b.EventLogId=l.Id
	inner join [dbo].[BuildDocuments] d with(nolock) on d.BuildId=b.Id and d.UpdatedBy is null
	inner join [dbo].[CodeDocuments] c with(nolock) on c.Id=d.DocumentId

end

/*
exec [dbo].[GetBuildDocUtilBatch] @UserIds='2,3,4,5,6,7,8,9,10'
*/