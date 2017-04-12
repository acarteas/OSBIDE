namespace OSBIDE.Data.SQLDatabase
{
    public static class SQLTemplatePassiveSocialEvents
    {
        public const string Insert =
@"if exists(select id from OsbideUsers where id=USERID) and not exists(select 1 from DESTINATIONTABLE where [UserId]=USERID and [ControllerName]='CONTROLLERNAME' and [ActionName]='ACTIONNAME' and [AccessDate]='ACCESSDATE')
begin
insert into DESTINATIONTABLE
([UserId], [ControllerName], [ActionName], [ActionParameter1], [ActionParameter2], [ActionParameter3], [AccessDate], [ActionParameters], [EventCode])
values
(USERID, 'CONTROLLERNAME', 'ACTIONNAME', ACTIONPARAM1, ACTIONPARAM2, ACTIONPARAM3, 'ACCESSDATE', ACTIONPARAMS, 'EVENTCODE')
end";
    }
}
