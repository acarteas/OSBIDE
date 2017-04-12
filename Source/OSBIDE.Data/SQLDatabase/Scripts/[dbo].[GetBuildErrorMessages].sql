-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetBuildErrorMessages]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetBuildErrorMessages]

	@buildIds nvarchar(max)
as
begin

	set nocount on;

	declare @logs table(EventLogId int)
	insert into @logs select buildId=cast(items as int) from [dbo].[Split](@buildIds, ',')

	select LogId=b.EventLogId, ErrorMessage=eli.[Description]
	from @logs b
	inner join [dbo].[BuildEvents] be on be.EventLogId=b.EventLogId
	inner join [dbo].[BuildEventErrorListItems] beeli on beeli.BuildEventId=be.Id
	inner join [dbo].[ErrorListItems] eli on eli.Id=beeli.ErrorListItemId

end
