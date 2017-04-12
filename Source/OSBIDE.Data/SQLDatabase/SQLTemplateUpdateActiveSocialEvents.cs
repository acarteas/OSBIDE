namespace OSBIDE.Data.SQLDatabase
{
    public class SQLTemplateUpdateActiveSocialEvents
    {
        public static string Template
        {
            get
            {
                return @"insert into [dbo].[ActiveSocialEvents]
(EventLogId, SenderId, EventDate, SolutionName, LogType, BuildErrorLogId, ExecutionActionId, FirstName, LastName, InstitutionId, MarkerType)
select distinct EventLogId=l.Id, l.SenderId, v.EventDate, v.SolutionName, l.LogType, BuildErrorLogId=be.LogId, d.ExecutionAction, u.FirstName, u.LastName, u.InstitutionId
, MarkerType=case when l.LogType='AskForHelpEvent' or p.Comment like '%?%' then 'QP'
					when p.Comment not like '%?%' then 'NP'

					when ol.SenderId<>l.SenderId and p.Comment like '%?%' then 'RQ'
					when ol.SenderId<>l.SenderId and p.Comment not like '%?%' then 'RN'

					when ol.SenderId=l.SenderId and p.Comment like '%?%' then 'QF'
					when ol.SenderId=l.SenderId and p.Comment not like '%?%' then 'NF'

					when sl.SenderId<>l.SenderId and sp.Comment like '%?%' then 'QR'
					when sl.SenderId<>l.SenderId and sp.Comment not like '%?%' then 'NR'

					when sl.SenderId=l.SenderId and sp.Comment like '%?%' then 'QF'
					when sl.SenderId=l.SenderId and sp.Comment not like '%?%' then 'NF'
				end
from [dbo].[OsbideUsers] u
inner join [dbo].[EventLogs] l on l.SenderId=u.Id
inner join [dbo].[EventTimeView] v on v.EventLogId=l.Id
left join [dbo].[BuildErrors] be on be.LogId=l.Id
left join [dbo].[DebugEvents] d on d.EventLogId=l.Id
-- subject user's posts with answers
left join ([dbo].[FeedPostEvents] p
			inner join [dbo].[EventLogs] ol on ol.Id=p.EventLogId
			left join [dbo].[LogCommentEvents] oc on oc.EventLogId=ol.Id) on p.EventLogId=l.Id
-- subject user's comments on posts
left join ([dbo].[LogCommentEvents] sc
			inner join [dbo].[EventLogs] sl on sl.Id=sc.SourceEventLogId
			inner join [dbo].[FeedPostEvents] sp on sp.EventLogId=sl.Id) on sc.EventLogId=l.Id
-- subject user's comments on ask for help
left join ([dbo].[LogCommentEvents] hc
			inner join [dbo].[EventLogs] hl on hl.Id=hc.SourceEventLogId
			inner join [dbo].[AskForHelpEvents] hp on hp.EventLogId=hl.Id) on hc.EventLogId=l.Id
where l.Id >={0} and l.Id < {1}";
            }
        }
    }
}
