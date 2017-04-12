using System.Linq;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class ActiveSocialEventUtilProc
    {
        private const int BATCH_SIZE = 10;

        public static bool Run()
        {
            using (var context = new OsbideProcs())
            {
                var prevProcessInfo = context.GetActiveSocialEventProcessInfo().ToList();
                var pocessedLogId = prevProcessInfo.First().Info;
                var maxUnprocessedLogId = prevProcessInfo.Last().Info;

                var batches = (maxUnprocessedLogId - pocessedLogId) / BATCH_SIZE;
                for (var b = 0; b < batches + 1; b++)
                {
                    var firstRow = pocessedLogId + b * BATCH_SIZE;
                    var lastRow = pocessedLogId + (b + 1) * BATCH_SIZE < maxUnprocessedLogId ? pocessedLogId + (b + 1) * BATCH_SIZE : maxUnprocessedLogId + 1;

                    DynamicSQLExecutor.Execute(string.Format(SQLTemplateUpdateActiveSocialEvents.Template, firstRow, lastRow));
                }

                return true;
            }
        }
    }
}
