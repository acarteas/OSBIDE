---------------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------------
-- sproc [dbo].[GetCalendarMeasuresByDay]
---------------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------------
/*

 dbcc freeproccache with no_infomsgs;

 exec [dbo].[GetCalendarMeasuresByDay] @measures='ActiveStudents,LinesOfCodeWritten,TimeSpent,NumberOfCompilations,NumberOfErrorsPerCompilation,NumberOfNoDebugExecutions,NumberOfDebugExecutions,NumberOfBreakpointsSet,NumberOfRuntimeExceptions,NumberOfPosts,NumberOfReplies,TimeToFirstReply'
 ,@courseId=4
 ,@isAvg=1

 exec [dbo].[GetCalendarMeasuresByDay] @measures='ActiveStudents,LinesOfCodeWritten,TimeSpent,NumberOfCompilations,NumberOfErrorsPerCompilation,NumberOfNoDebugExecutions,NumberOfDebugExecutions,NumberOfBreakpointsSet,NumberOfRuntimeExceptions,NumberOfPosts,NumberOfReplies,TimeToFirstReply'
 ,@courseId=4
 ,@isAvg=0
 ,@students='30,86,90,139,208,222,255'

exec [dbo].[GetCalendarMeasuresByDay] @measures='ActiveStudents,LinesOfCodeWritten,NumberOfErrorsPerCompilation'
 ,@startDate='2014-02-01 00:00:00'
 ,@endDate='2014-04-01 00:00:00'
 ,@students='30,86,90,139,208,222,255'
 ,@courseId=4
 ,@isAvg=0

*/
alter procedure [dbo].[GetCalendarMeasuresByDay]

	 @startDate date='02-01-2014'
	,@endDate date='04-01-2014'
	,@students varchar(max)=''
	,@courseId int=-1
	,@measures varchar(2000)=''
	,@isAvg bit=0
as
begin

--declare @startDate datetime='01-01-2014'
--declare @endDate datetime='03-01-2014'
--declare @students varchar(max)=''
--declare @isAvg bit=1

	set nocount on;

	--supplying a data contract for EF
	if 1 = 2 begin
		select
			cast(null as date)			as EventDay
		   ,cast(null as int)			as Value
		   ,cast(null as varchar(200))	as Measure
		where
			1 = 2  
	end

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Base events
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- the @students list is never empty since the view is launched for a specific search result set
	declare @UserIdsTable table (userId int)
	insert into @UserIdsTable
	select userId=cast(items as int) from [dbo].[Split](isNull(@students, ''), ',')

	if object_id('tempdb..#EventLogs') is not null
		drop table #EventLogs

	create table #EventLogs
	(
		EventLogId int,
		EventTypeId int,
		EventDate datetime,
		UserId int,
		BuildEventId int
	)

	insert into #EventLogs
	select EventLogId=l.Id, l.EventTypeId, EventDate=ev.EventDate, UserId=l.SenderId, BuildEventId=b.Id
	from [dbo].[EventLogs] l
	inner join [dbo].[EventTimeView] ev on ev.EventLogId=l.Id
	inner join @UserIdsTable u on u.userId=l.SenderId
	left join [dbo].[BuildEvents] b on b.EventLogId=l.Id
	where EventTypeId in (1, 2, 4, 6, 7, 9, 10) and ev.EventDate between @startDate and @endDate

	create nonclustered index TempEventLogs
	on [dbo].[#EventLogs] ([EventTypeId],[EventDate],[BuildEventId])
	include ([UserId])

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Base programming events with event timespane of users (learn about lag)
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	if object_id('tempdb..#ProgrammingEvents') is not null
		drop table #ProgrammingEvents

	select UserId, EventTypeId, EventDate,
			lag(EventDate, 1, null) over(partition by UserId order by EventDate desc) as NextEventTime,
			EventDay=convert(Date, eventDate)
	into #ProgrammingEvents
	from #EventLogs
	where EventTypeId in (2, 4, 10) and EventDate between @startDate and @endDate
	order by UserId, EventDate desc

	create nonclustered index TempProgrammingEvents
	on [dbo].[#ProgrammingEvents] ([NextEventTime])
	include ([UserId],[EventDate],[EventDay])

	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
	-- Return measures
	-------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------
		if object_id('tempdb..#Measures') is not null
			drop table #Measures

		create table #Measures
		(
			EventDay date,
			Value int,
			Measure varchar(200)
		)

		-- Assignments
		-------------------------------------------------------------------------------------------------
			insert into #Measures
			select EventDay=convert(date, a.ReleaseDate), Value=-1, Measure=c.Prefix + ' ' + c.CourseNumber + ' ' + a.Name + ' Released'
			from [dbo].[Assignments] a
			inner join [dbo].[Courses] c on c.Id=a.CourseId and c.IsDeleted=0
			where a.IsDeleted=0 and (@courseId=-1 or @courseId=c.Id) and a.ReleaseDate between @startDate and @endDate

			insert into #Measures
			select EventDay=convert(date, a.DueDate), Value=-2, Measure=c.Prefix + ' ' + c.CourseNumber + ' ' + a.Name + ' Due'
			from [dbo].[Assignments] a
			inner join [dbo].[Courses] c on c.Id=a.CourseId and c.IsDeleted=0
			where a.IsDeleted=0 and (@courseId=-1 or @courseId=c.Id) and a.DueDate between @startDate and @endDate

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Active Students: distinct student counts for a day
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if @isAvg=0 and charindex('ActiveStudents', @measures) > 0
			insert into #Measures
			select EventDay, Value=count(distinct UserId), Measure='ActiveStudents'
			from #ProgrammingEvents
			where NextEventTime is not null and datediff(second, eventDate, NextEventTime) < 6
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Time Spent: total compiling related event timespans, timespan > 5 second count as idle
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('TimeSpent', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(TotalTimeSpent) else sum(TotalTimeSpent) end), Measure='TimeSpent'
			from
			(
				select EventDay, UserId, TotalTimeSpent=sum(datediff(second, EventDate, NextEventTime))/60
				from #ProgrammingEvents
				where NextEventTime is not null and datediff(second, EventDate, NextEventTime) < 6
				group by EventDay, UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Number of Compilations: total compiling related event count
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfCompilations', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(TotalCompilations) else sum(TotalCompilations) end), Measure='NumberOfCompilations'
			from
			(
				select EventDay, UserId, TotalCompilations=count(*)
				from #ProgrammingEvents
				where NextEventTime is not null and datediff(second, EventDate, NextEventTime) < 6
				group by EventDay, UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Lines of Code Written: number of modified lines
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('LinesOfCodeWritten', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfModified) else sum(NumberOfModified) end), MeasureType='LinesOfCodeWritten'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId, NumberOfModified=sum(isNull(b.NumberOfModified,0))
				from [dbo].[BuildDocuments] b
				inner join #EventLogs l on l.BuildEventId=b.BuildId
				group by convert(Date, l.EventDate), l.UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Number of Errors Per Compilations: total build errors
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfErrorsPerCompilation', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(CompilationErrors) else sum(CompilationErrors) end), Measure='NumberOfErrorsPerCompilation'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId, b.LogId, CompilationErrors=count(b.BuildErrorTypeId)
				from [dbo].[BuildErrors] b
				inner join #EventLogs l on l.EventLogId=b.LogId
				group by convert(Date, l.EventDate), l.UserId, b.LogId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Avg. number of executions without debug per (active) student
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfNoDebugExecutions', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfExecutions) else sum(NumberOfExecutions) end), Measure='NumberOfNoDebugExecutions'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId,
						NumberOfExecutions=sum(case when e.ExecutionAction=5 then 1 else 0 end)
				from [dbo].[DebugEvents] e
				inner join #EventLogs l on l.EventLogId=e.EventLogId
				group by convert(Date, l.EventDate), l.UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Avg. number of executions with debug per (active) student
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfDebugExecutions', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfDebuggings) else sum(NumberOfDebuggings) end), Measure='NumberOfDebugExecutions'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId,
						NumberOfDebuggings=sum(case when e.ExecutionAction=0 then 1 else 0 end)
				from [dbo].[DebugEvents] e
				inner join #EventLogs l on l.EventLogId=e.EventLogId
				group by convert(Date, l.EventDate), l.UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Avg. number of breakpoints set per (active) student
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfBreakpointsSet', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfBreakPoints) else sum(NumberOfBreakPoints) end), Measure='NumberOfBreakpointsSet'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId, /*b.BuildEventId,*/ NumberOfBreakPoints=count(b.BreakPointId)
				from [dbo].[BuildEventBreakPoints] b
				inner join #EventLogs l on l.BuildEventId=b.BuildEventId
				group by convert(Date, l.EventDate), l.UserId/*, b.BuildEventId*/
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Avg. number of runtime exceptions obtained per (active) student
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfRuntimeExceptions', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfExceptions) else sum(NumberOfExceptions) end), Measure='NumberOfRuntimeExceptions'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId, NumberOfExceptions=count(l.EventLogId)
				from #EventLogs l
				where l.EventTypeId=6
				group by convert(Date, l.EventDate), l.UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Number of new threads started
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfPosts', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfNewThread) else sum(NumberOfNewThread) end), Measure='NumberOfPosts'
			from
			(
				select EventDay=convert(Date, l.EventDate), l.UserId, NumberOfNewThread=count(l.EventLogId)
				from #EventLogs l
				where EventTypeId in (1, 7)
				group by convert(Date, l.EventDate), l.UserId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Number of replies (when averaging, do on per thread basis)
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('NumberOfReplies', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(NumberOfReplies) else max(NumberOfReplies) end), Measure='NumberOfReplies'
			from
			(
				select EventDay=convert(Date, e.EventDate), l.EventLogId, NumberOfReplies=count(e.EventLogId)
				from #EventLogs l
				inner join [dbo].[LogCommentEvents] e on e.SourceEventLogId=l.EventLogId
				where l.EventTypeId in (1, 7)
				group by convert(Date, e.EventDate), l.EventLogId
			)
			sub
			group by EventDay

		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		-- Time to first reply (when averaging, do on per thread basis)
		-------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------
		if charindex('TimeToFirstReply', @measures) > 0
			insert into #Measures
			select EventDay, Value=(case when @isAvg=1 then avg(FirstReplyTime) else min(FirstReplyTime) end), Measure='TimeToFirstReply'
			from
			(
				select EventDay=convert(Date, e.EventDate), FirstReplyTime=min(datediff(second, e.EventDate, r.EventDate))/60
				from #EventLogs e
				inner join [dbo].[LogCommentEvents] r on r.SourceEventLogId=e.EventLogId
				where e.EventTypeId in (1, 7)
				group by convert(Date, e.EventDate)
			)
			sub
			group by EventDay


		-- proc return
		-------------------------------------------------------------------------------------------------
		select * from #Measures
		order by Measure, EventDay

end



