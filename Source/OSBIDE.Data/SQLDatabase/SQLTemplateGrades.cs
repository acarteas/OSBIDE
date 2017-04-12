namespace OSBIDE.Data.SQLDatabase
{
    public static class SQLTemplateGrades
    {
        public const string Upsert = @"
if exists(select 1 from [dbo].[StudentGrades] where StudentId=C_1 and CourseId=K1 and Deliverable='K2')
    update [dbo].[StudentGrades] set grade=C_2, UpdatedOn='K3', UpdatedBy=K4 where StudentId=C_1 and CourseId=K1 and Deliverable='K2'
else
    insert into [dbo].[StudentGrades] ([StudentId],[CourseId],[Deliverable],[Grade],[CreatedOn],[CreatedBy]) values (C_1,K1,'K2',C_2,'K3',K4)NEW_LINENEW_LINE";
    }
}
