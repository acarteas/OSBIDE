-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [InsertPostTags]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[InsertPostTags]

	 @postId int
	,@usertags varchar(max)
	,@hashtags varchar(max)

as
begin

	set nocount on;

	if @hashtags is not null
	begin
	declare @hashtagTable table(Tag varchar(200), isInTable bit)
	insert into @hashtagTable 
	select Tag=items, isInTable = case when b.Content is null then 0 else 1 end
	from dbo.Split(@hashtags, ',') a
	left join dbo.HashTags b on b.Content = a.items
	 
	insert into dbo.HashTags select content = tag from @hashtagTable where isInTable = 0

	insert into FeedPostHashtags
	select FeedPostId = @postId, HashTagId = b.Id
	from @hashtagTable a
	inner join dbo.HashTags b on b.Content = a.Tag
	end


	if @usertags is not null
	begin
	declare @nameTable table(name varchar(200))
	insert into @nameTable
	select name = items 
	from dbo.Split(@usertags, ',')

	insert into FeedPostUserTags
	select FeedPostId = @postId, UserId = b.Id, Viewed = 0
	from @nameTable a
	inner join dbo.OsbideUsers b on b.FirstName + b.LastName = a.name
	end
end