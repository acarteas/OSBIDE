--for posts
use osbide_analytics_debug
go
delete FROM Posts
go
INSERT INTO Posts (OsbideId, AuthorId, DatePosted, Content, ParentId)
SELECT el.Id, el.SenderId, el.DateReceived, fpe.Comment, -1
FROM [2015-01-01-osbide].dbo.FeedPostEvents fpe
INNER JOIN [2015-01-01-osbide].dbo.EventLogs el ON fpe.EventLogId = el.Id
go

--for replies
INSERT INTO Posts (OsbideId, AuthorId, DatePosted, Content, ParentId)
SELECT el.Id, el.SenderId, el.DateReceived, lce.Content, p.Id
FROM [2015-01-01-osbide].dbo.LogCommentEvents lce
INNER JOIN [2015-01-01-osbide].dbo.EventLogs el ON lce.EventLogId = el.Id
INNER JOIN Posts p ON lce.SourceEventLogId = p.OsbideId
go

--for counts
INSERT INTO QuestionResponseCounts(ContentId, Count)
SELECT Id, 0 FROM Posts
go