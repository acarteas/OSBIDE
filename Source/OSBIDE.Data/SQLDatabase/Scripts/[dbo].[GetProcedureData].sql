-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetProcedureData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetProcedureData]

	 @dateFrom DateTime
	,@dateTo DateTime
	,@studentId int
	,@nameToken nvarchar(255)
	,@gender int
	,@ageFrom int
	,@ageTo int
	,@courseId int
	,@deliverable nvarchar(255)
	,@gradeFrom decimal
	,@gradeTo decimal
as
begin

	set nocount on;

	declare @minDate datetime='1-1-2010'
	declare @anyValue int=-1
	if datediff(day, @dateFrom, @dateTo)=0 and @dateTo>@minDate set @dateTo=dateadd(day, 1, @dateTo)

	--declare @dateFrom datetime='1-1-2000'
	--declare @dateTo datetime=getDate()
	--declare @studentId int=3
	--declare @gender int=1
	--declare @courseId int=1
	--declare @gradeFrom decimal=60
	--declare @gradeTo decimal=100
	--declare @ageFrom int=16
	--declare @ageTo int=30
	--declare @deliverable nvarchar(255)='homework 1'
	--declare @nameToken nvarchar(255)='ro'
	
	create table #temp
	(
		UserId int,
		InstitutionId int,
		FirstName nvarchar(255),
		LastName nvarchar(255),
		Age int,
		Gender int,
		Ethnicity nvarchar(255),
		Prefix nvarchar(255),
		CourseNumber nvarchar(255),
		Season nvarchar(255),
		[Year] int,
		Deliverable nvarchar(255),
		Grade decimal,
		LastActivity datetime
	)

	insert into #temp
	select UserId=u.Id, u.InstitutionId, u.FirstName, u.LastName,
		   s.Age, Gender=u.GenderValue, Ethnicity=s.Ethnicity_Ethn,
		   c.Prefix, c.CourseNumber, c.Season, c.[Year],
		   g.Deliverable, g.Grade, LastActivity=max(e.DateReceived)
	from [dbo].[OsbideUsers] u with (nolock)
	inner join [dbo].[OsbideSurveys] s with (nolock) on s.UserInstitutionId=u.InstitutionId
													and (u.InstitutionId=@studentId or @studentId=@anyValue)
													and (s.Age between @ageFrom and @ageTo
													     or @ageFrom=@anyValue and s.Age<=@ageTo
														 or @ageTo=@anyValue and s.Age>=@ageFrom
														 or @ageFrom=@anyValue and @ageTo=@anyValue)
	inner join [dbo].[EventLogs] e with (nolock) on e.SenderId=u.Id
													and (e.DateReceived between @dateFrom and @dateTo
													     or @dateFrom<@minDate and e.DateReceived<=@dateTo
														 or @dateTo<@minDate and e.DateReceived>=@dateFrom
														 or @dateFrom<@minDate and @dateTo<@minDate)
	inner join [dbo].[StudentGrades] g with (nolock) on g.StudentId=u.InstitutionId
													and (g.CourseId=@courseId or @courseId=@anyValue)
													and (g.Deliverable=@deliverable or @deliverable='Any')
													and (g.Grade between @gradeFrom and @gradeTo
													     or @gradeFrom=@anyValue and g.Grade<=@gradeTo
														 or @gradeTo=@anyValue and g.Grade>=@gradeFrom
														 or @gradeFrom=@anyValue and @gradeTo=@anyValue)
	inner join [dbo].[Courses] c with (nolock) on c.Id=g.CourseId
	where u.RoleValue=1 and (u.GenderValue=@Gender or @Gender=1)
	group by u.Id, u.InstitutionId, u.FirstName, u.LastName, s.Age, u.GenderValue, s.Ethnicity_Ethn, c.Prefix, c.CourseNumber, c.Year, c.Season, g.Deliverable, g.Grade

	create table #ret
	(
		UserId int,
		InstitutionId int,
		FirstName nvarchar(255),
		LastName nvarchar(255),
		Age int,
		Gender int,
		Ethnicity nvarchar(255),
		Deliverable nvarchar(255),
		Grade decimal,
		LastActivity datetime,
		Prefix nvarchar(255),
		CourseNumber nvarchar(255),
		Season nvarchar(255),
		[Year] int
	)

	if len(ltrim(rtrim(@nameToken)))=0
		insert into #ret
		select UserId, InstitutionId, FirstName , LastName, Age, Gender, Ethnicity, Deliverable, Grade, LastActivity
				,Prefix, CourseNumber, Season, [Year]=cast([Year] as varchar(4))
		from #temp
	else
	begin
		set @nameToken='%' + @nameToken + '%'
		insert into #ret
		select UserId, InstitutionId, FirstName, LastName, Age, Gender, Ethnicity, Deliverable, Grade, LastActivity
			  ,Prefix, CourseNumber, Season, [Year]=cast([Year] as varchar(4))
		from #temp where firstName like @nameToken or LastName like @nameToken
	end

	select UserId, InstitutionId, FirstName , LastName, Age, Gender, Ethnicity, Grade=100, LastActivity=max(LastActivity), Prefix, CourseNumber, Season, [Year],
			Deliverable=stuff(
				(select distinct ', ' + Deliverable + ':' + cast(Grade as varchar(20))
					from #ret
					where UserId=a.UserId and InstitutionId=a.InstitutionId
						and FirstName=a.FirstName and LastName=a.LastName and Age=a.Age
						and Gender=a.Gender and Ethnicity=a.Ethnicity
						and Prefix=a.Prefix and CourseNumber=a.CourseNumber and Season=a.Season and [Year]=a.[Year]
					for xml path ('')), 1, 1,'')
	from #ret a
	group by UserId, InstitutionId, FirstName , LastName, Age, Gender, Ethnicity, Prefix, CourseNumber, Season, [Year]

end


/*

exec [dbo].[GetProcedureData] @dateFrom='2000-01-01 00:00:00',
@dateTo='2014-05-03 00:00:00',
@nameToken='',@Gender=1,@ageFrom=17,@ageTo=28
@courseId=-1,@deliverable=N'Any',@gradeFrom=0,@gradeTo=0

select * from [dbo].[Courses]


		select UserId=1
		,InstitutionId=1
		,FirstName='test'
		,LastName='test'
		,Age=1
		,Gender=1
		,Ethnicity='test'
		,Deliverable='test'
		,Grade=3.2
		,LastActivity=getDate()
		,Prefix='test'
		,CourseNumber='test'
		,Season='test'
		,[Year]=100
exec [dbo].[GetProcedureData] @dateFrom='2014-01-17 00:00:00',@dateTo='2014-01-17 00:00:00',@studentId=-1,@nameToken=N'',@gender=1,@ageFrom=-1,@ageTo=-1,@courseId=-1,@deliverable=N'Any',@gradeFrom=-1,@gradeTo=-1

*/

