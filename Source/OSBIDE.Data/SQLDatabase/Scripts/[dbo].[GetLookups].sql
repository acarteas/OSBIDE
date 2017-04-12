-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc get criteria lookups
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------

create procedure [dbo].[GetAgeLookup]

as
begin

	set nocount on;
	select distinct Age as Age from [dbo].[OsbideSurveys] with (nolock) where Age is not null

end
go

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetCourseLookup]

as
begin

	set nocount on;

	select CourseId=Id, CourseName=Prefix + ' ' + CourseNumber + ', ' + Season + ' ' + cast([Year] as varchar(4))
	from [dbo].[Courses]

end
go

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
alter procedure [dbo].[GetDeliverableLookup]

	 @courseId int
	,@dateFrom datetime
	,@dateTo datetime
as
begin

	set nocount on;

	select Deliverable=Name
	from [dbo].[Assignments] with (nolock)
	where CourseId=@courseId and IsDeleted=0 and ReleaseDate > @dateFrom and ReleaseDate <= @dateTo

end
go










