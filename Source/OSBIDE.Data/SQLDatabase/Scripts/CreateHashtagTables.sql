IF NOT EXISTS (
	SELECT 1 FROM sys.tables t
	INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
	WHERE t.name='HashTags' AND s.name='dbo')
	CREATE TABLE [dbo].[HashTags](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Content] [nvarchar](800) NOT NULL,
		CONSTRAINT [PK_dbo.Hashtags] PRIMARY KEY CLUSTERED ([Id] ASC)
	)
GO

IF NOT EXISTS (
	SELECT 1 FROM sys.tables t
	INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
	WHERE t.name='FeedPostHashtags' AND s.name='dbo')
	CREATE TABLE [dbo].[FeedPostHashtags](
		[FeedPostId] [int] NOT NULL,
		[HashtagId] [int] NOT NULL,
		CONSTRAINT [PK_dbo.FeedPostHashtags] PRIMARY KEY (FeedPostId,HashtagId),
		CONSTRAINT [FK_dbo.FeedPostHashtags_EventLogs] FOREIGN KEY (FeedPostId) REFERENCES [dbo].[EventLogs] (Id),
		CONSTRAINT [FK_dbo.FeedPostHashtags_Hashtags] FOREIGN KEY (HashtagId) REFERENCES [dbo].[Hashtags] (Id)
	)
GO

IF NOT EXISTS (
	SELECT 1 FROM sys.tables t
	INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
	WHERE t.name='FeedPostUserTags' AND s.name='dbo')
	CREATE TABLE [dbo].[FeedPostUserTags](
		[FeedPostId] [int] NOT NULL,
		[UserId] [int] NOT NULL,
		[Viewed] [bit] NOT NULL,
		CONSTRAINT [PK_dbo.FeedPostUserTags] PRIMARY KEY (FeedPostId,UserId),
		CONSTRAINT [FK_dbo.FeedPostUserTags_EventLogs] FOREIGN KEY (FeedPostId) REFERENCES [dbo].[EventLogs] (Id),
		CONSTRAINT [FK_dbo.FeedPostUserTags_OsbideUsers] FOREIGN KEY (UserId) REFERENCES [dbo].[OsbideUsers] (Id)
	)
GO