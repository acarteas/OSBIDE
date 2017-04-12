delete from [dbo].[StudentGrades]
DBCC CHECKIDENT ([dbo.StudentGrades], reseed, 0)

declare @today datetime=getDate()
--insert into [dbo].[StudentGrades] ([StudentId], [Deliverable], [Grade], [CourseId], CreatedOn, CreatedBy)
select distinct u.Id, a.Name, case when (u.InstitutionId % 100) > 60 then (u.InstitutionId % 100) else 80 + u.Id % 10 end, c.Id, @today, 999
from [dbo].[Courses] c
inner join [dbo].[CourseUserRelationships] cr on cr.CourseId=c.Id and cr.IsActive=1 and cr.IsApproved=1
inner join [dbo].[OsbideUsers] u on u.Id=cr.UserId and u.RoleValue=1
inner join [dbo].[Assignments] a on a.CourseId=c.Id
where c.IsDeleted=0
order by u.InstitutionId, a.Name, c.Id




