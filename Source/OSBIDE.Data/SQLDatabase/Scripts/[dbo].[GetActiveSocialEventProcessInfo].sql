
alter procedure [dbo].[GetActiveSocialEventProcessInfo]
as
begin

	set nocount on;

	select Info=isNull(max(EventLogId),0) from [dbo].[ActiveSocialEvents]
	union all
	select Info=max(Id) from [dbo].[EventLogs]

end

