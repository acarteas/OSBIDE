--copies content coding into analytics db
--copies content coding into analytics db
use osbide_analytics_debug
go
delete FROM ContentCodings
go
INSERT INTO ContentCodings ([PostId]
      ,[Date]
      ,[AuthorId]
      ,[AuthorRole]
      ,[Content]
      ,[ReadingEase]
      ,[ReadingGradeLevel]
      ,[ToCode]
      ,[Category]
      ,[SubCategory]
      ,[PrimaryModifier]
      ,[SecondaryModifier]
      ,[TertiaryModifier]
      ,[CodeModifier]
      ,[HelpAcknowledged])
SELECT [Post ID]
      ,[Date]
      ,[Author]
      ,[AuthorRole]
      ,[Content]
      ,[Reading Ease]
      ,[Reading Grade Level]
      , CASE WHEN [Code this?] IS NULL THEN 1 ELSE 0 END AS ToCode
      , ISNULL([Cat], '') AS Category
      , ISNULL([SubCat], '') AS SubCategory
      , ISNULL([Mod], '') AS PrimaryModifier
      , ISNULL([Mod 2], '') AS SecondaryModifier
      , ISNULL([Mod 3], '') AS TertiaryModifier
      , ISNULL([CodeMod], '') AS CodeModifier
      , ISNULL([Help Ack], 0) AS PrimaryModifier 
  FROM [dissertation].[dbo].[2014_122_spring_feed_analysis]
  WHERE [Post ID] IS NOT NULL
go