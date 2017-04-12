-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetErrorQuotientDocumentData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetErrorQuotientDocumentData]

	 @buildIds nvarchar(max)
as
begin

	set nocount on;

	declare @builds table(buildId int)
	insert into @builds select buildId=cast(items as int) from [dbo].[Split](@buildIds, ',')

	select distinct d.BuildId, d.DocumentId, el.[Column], el.Line, [FileName]=cd.[FileName], d.NumberOfModified, d.ModifiedLines
	from @builds b
	inner join [dbo].[BuildDocuments] d with(nolock) on d.BuildId=b.BuildId
	inner join [dbo].[CodeDocuments] cd with(nolock) on cd.Id=d.DocumentId
	inner join [dbo].[CodeDocumentErrorListItems] celi with(nolock) on celi.CodeFileId=cd.Id
	inner join [dbo].[ErrorListItems] el with(nolock) on el.Id=celi.ErrorListItemId

end

