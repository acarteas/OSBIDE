
create table [dbo].[PassiveSocialEvents](
	[Id] [int] identity(1,1) not null,
	[UserId] [int] not null,
	[ControllerName] [nvarchar](max) NULL,
	[ActionName] [nvarchar](max) NULL,
	[ActionParameter1] [nvarchar](max) NULL,
	[ActionParameter2] [nvarchar](max) NULL,
	[ActionParameter3] [nvarchar](max) NULL,
	[ActionParameters] [nvarchar](max) NULL,
	[EventCode] [nvarchar](8) NULL,
	[AccessDate] [datetime] not null,
	constraint [PK_dbo.ActionRequestLogsSmall] primary key clustered ([Id] asc),
	constraint [FK_dbo.ActionRequestLogs_dbo.OsbideUsers_CreatorId_small] foreign key ([UserId]) references [dbo].[OsbideUsers] ([Id])
)

insert into [dbo].[PassiveSocialEvents]
([UserId], [ControllerName], [ActionName], [ActionParameter1], [ActionParameter2], [ActionParameter3], [AccessDate], [ActionParameters], [EventCode])
select [UserId]=CreatorId, a.ControllerName, a.ActionName, [ActionParameter1], [ActionParameter2], [ActionParameter3], a.[AccessDate], [ActionParameters],
[EventCode]=case when ControllerName='Assignment' and ActionName='DownloadStudentAssignment' then 'AD'
				 when ControllerName='BuildEvent' and ActionName='Diff' then 'BD'
				 when ControllerName='Course' and ActionName='Details' then 'CD'
				 when ControllerName='Course' and ActionName='Index' then 'CI'
				 when ControllerName='Course' and ActionName='MakeDefault' then 'CM'
				 when ControllerName='Course' and ActionName='Search' then 'CS'
				 when ControllerName='Feed' and ActionName='ApplyFeedFilter' then 'FA'
				 when ControllerName='Feed' and ActionName='Details' then 'FD'
				 when ControllerName='Feed' and ActionName='FollowPost' then 'FF'
				 when ControllerName='Feed' and ActionName='GetComments' then 'FG'
				 when ControllerName='Feed' and ActionName='Index' and ActionParameter1='timestamp=-1' and ActionParameter2='errorType=-1' and ActionParameter3='errorTypeStr=' then 'FI'
				 when ControllerName='Feed' and ActionName='Index' and len(isNull(ActionParameter2,''))>0 and ActionParameter2!='errorType=-1' then 'FEO'
				 when ControllerName='Feed' and ActionName='Index' and len(isNull(ActionParameter3,''))>0 and ActionParameter3!='errorTypeStr=' then 'FEW'
				 when ControllerName='Feed' and ActionName='MarkCommentHelpful' then 'FM'
				 when ControllerName='Feed' and ActionName='OldFeedItems' then 'FO'
				 when ControllerName='Feed' and ActionName='UnfollowPost' then 'FU'
				 when ControllerName='File' and ActionName='GetAssignmentAttachment' then 'FA'
				 when ControllerName='File' and ActionName='GetCourseDocument' then 'FC'
				 when ControllerName='Profile' and ActionName='Edit' then 'PE'
				 when ControllerName='Profile' and ActionName='Index' then 'PI'
			end
from [dbo].[ActionRequestLogs] a
cross apply (select charindex('|||',isNull(ActionParameters, '')) as p1)p
cross apply (select case when len(isNull(ActionParameters, ''))=0 then '' else substring(ActionParameters,1,p.p1-1) end as [ActionParameter1])s
cross apply (select charindex('|||',isNull(ActionParameters, ''),p.p1+3) as p2)q
cross apply (select case when len(isNull(ActionParameters, ''))=0 or q.p2=0 then '' else substring(ActionParameters,p.p1+3,q.p2-p.p1-3) end as [ActionParameter2])t
cross apply (select case when len(isNull(ActionParameters, ''))=0 or q.p2=0 or len(ltrim(rtrim(ActionParameters)))-q.p2<6 then '' else substring(ActionParameters,q.p2+3,len(ltrim(rtrim(ActionParameters)))-q.p2-5) end as [ActionParameter3])u
where
(  ControllerName='Assignment' and ActionName='DownloadStudentAssignment'
	or ControllerName='BuildEvent' and ActionName='Diff'
	or ControllerName='Course' and (ActionName='Details' or ActionName='Index' or ActionName='MakeDefault' or ActionName='Search')
	or ControllerName='Feed' and (ActionName='ApplyFeedFilter'
									or ActionName='Details'
									or ActionName='FollowPost'
									or ActionName='GetComments' and ActionParameters like '%singleLogId%' and ActionParameters != 'singleLogId=[null]|||'
									or ActionName='Index'
									or ActionName='MarkCommentHelpful'
									or ActionName='OldFeedItems'
									or ActionName='UnfollowPost')
	or ControllerName='File' and (ActionName='GetAssignmentAttachment' or ActionName='GetCourseDocument')
	or ControllerName='Profile' and (ActionName='Edit' or ActionName='Index')
)



if exists(select 1 from sys.indexes where name='IX_VisualizationQuery_PassiveSocialEvents_UserId' and object_id=object_id('PassiveSocialEvents'))
	drop index [IX_VisualizationQuery_PassiveSocialEvents_UserId] on [dbo].[PassiveSocialEvents]

create nonclustered index [IX_VisualizationQuery_PassiveSocialEvents_UserId] on [dbo].[PassiveSocialEvents]([UserId] asc)
include ([EventCode], [AccessDate])


--exec [dbo].[GetStateMachineEvents] @dateFrom='2014-01-15 12:00:00',@dateTo='2014-01-31 12:00:00',@studentIds='68,95,157'


--select * from [dbo].[PassiveSocialEvents]
--where accessDate between '1-30-2014' and '2-1-2014' and userID=95