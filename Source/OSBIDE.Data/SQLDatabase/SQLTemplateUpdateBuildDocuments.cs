using System;

namespace OSBIDE.Data.SQLDatabase
{
    public class SQLTemplateUpdateBuildDocuments
    {
        public static string Template
        {
            get
            {
                return @"update [dbo].[BuildDocuments]
set NumberOfInserted={0},NumberOfModified={1},NumberOfDeleted={2},ModifiedLines='{3}',UpdatedOn='{4}',UpdatedBy={5}
where BuildId={6} and DocumentId={7}" + Environment.NewLine;
            }
        }
    }
}