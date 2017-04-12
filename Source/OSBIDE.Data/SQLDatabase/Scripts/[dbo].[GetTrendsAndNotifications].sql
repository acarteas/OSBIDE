-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetTrendsAndNotifications]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetTrendsAndNotifications]

	 @user int
	,@num int
	,@getAll bit
as
begin
	set nocount on;
	select top(@num) u.FirstName, u.LastName, EventLogId=e.Id, t.Viewed, UserId=u.Id, HashtagId=null, Hashtag=null, Counts=null
	from [dbo].[FeedPostUserTags] t
	inner join [dbo].[EventLogs] e on t.FeedPostId = e.Id
	inner join [dbo].[OsbideUsers] u on e.SenderId = u.Id
	where t.UserId = @user and ( t.Viewed = 0 or @getAll = 1)

	union all
	select top(@num) FirstName=null, LastName=null, EventLogId=null, Viewed=null, UserId=null, HashtagId, Hashtag, Counts
	from
	(
		select HashtagId=h.Id, Hashtag=h.Content, Counts=count(distinct e.Id)
		from [dbo].[FeedPostHashtags] t
		inner join [dbo].[HashTags] h on h.Id=t.HashtagId
		inner join [dbo].[FeedPostEvents] e on e.EventLogId = t.FeedPostId
		group by h.Id, h.Content
	)
	sub
	order by Counts desc

end