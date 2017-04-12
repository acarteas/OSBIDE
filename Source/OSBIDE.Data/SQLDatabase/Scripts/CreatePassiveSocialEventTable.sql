-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- create PassiveSocialEvents table
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
if not exists (
	select 1 from sys.tables t
	inner join sys.schemas s on s.schema_id=t.schema_id
	where t.name='PassiveSocialEvents' and s.name='dbo')

	begin

	create table [dbo].[PassiveSocialEvents]
	(
		[EventLogId] int not null,
		[UserId] int not null,
		[EventDate] [datetime] not null,
		constraint pk_PassiveSocialEvents primary key (EventLogId,UserId,EventDate)
	)

	end

