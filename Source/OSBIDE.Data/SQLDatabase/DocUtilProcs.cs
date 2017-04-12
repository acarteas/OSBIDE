using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class DocUtil
    {
        private const int BATCH_SIZE = 20;

        public static bool Run(int updatedBy)
        {
            var docUsers = DocUtilProcs.GetDocUsers();

            var batches = docUsers.Count / BATCH_SIZE;
            var now = DateTime.Now.ToString();

            for (var b = 0; b < batches + 1; b++)
            {
                var tsql = new StringBuilder();

                var firstRow = b * BATCH_SIZE;
                var lastRow = (b + 1) * BATCH_SIZE > docUsers.Count ? docUsers.Count : (b + 1) * BATCH_SIZE;

                // only pulling 20 users' build records each time
                // to fit Azure multi-tenant limitted connection time situation
                var docs = DocUtilProcs.GetDocs(docUsers.Skip(firstRow).Take(lastRow - firstRow))
                               .GroupBy(d => d.UserId).ToDictionary(d =>
                                new
                                {
                                    User = d.Key,
                                    Docs = d.Select(x => new { x.Document, x.SolutionName, x.FileName, x.BuildId, x.DocumentId }).ToList(),
                                });

                foreach (var u in docs.Keys)
                {
                    var orderedUserDocs = docs[u].OrderBy(d => d.FileName).OrderBy(d => d.EventDate).ToArray();

                    // every user's first build doc is new
                    tsql.AppendFormat(SQLTemplateUpdateBuildDocuments.Template, 0, 0, 0, string.Empty, now, updatedBy, orderedUserDocs[0].BuildId, orderedUserDocs[0].DocumentId);
                    for (var idx = 1; idx < orderedUserDocs.Count(); idx++)
                    {
                        if (string.Compare(orderedUserDocs[idx - 1].FileName, orderedUserDocs[idx].FileName, true) != 0)
                        {
                            // starting a different user build doc
                            tsql.AppendFormat(SQLTemplateUpdateBuildDocuments.Template, 0, 0, 0, string.Empty, now, updatedBy, orderedUserDocs[idx].BuildId, orderedUserDocs[idx].DocumentId);
                        }
                        else
                        {
                            // comparing this and the previous version of the build doc
                            var differ = new Differ();
                            var inlineBuilder = new SideBySideDiffBuilder(differ);
                            var result = inlineBuilder.BuildDiffModel(orderedUserDocs[idx - 1].Document, orderedUserDocs[idx].Document);
                            var deleted = result.OldText.Lines.Count(l => l.Type == ChangeType.Deleted);
                            var inserted = result.NewText.Lines.Count(l => l.Type == ChangeType.Inserted);
                            var modified = result.NewText.Lines.Count(l => l.Type == ChangeType.Modified);
                            var modifiedPositions = result.NewText.Lines.Where(l => l.Position.HasValue && l.Type == ChangeType.Modified).Select(l => l.Position.Value);

                            tsql.AppendFormat(SQLTemplateUpdateBuildDocuments.Template, inserted, modified, deleted, string.Join(",", modifiedPositions), now, updatedBy, orderedUserDocs[idx].BuildId, orderedUserDocs[idx].DocumentId);
                        }
                    }
                }

                DynamicSQLExecutor.Execute(tsql.ToString());
            }

            return true;
        }
    }
    public class DocUtilProcs
    {
        public static List<int> GetDocUsers()
        {
            using (var context = new OsbideProcs())
            {
                return (from p in context.GetBuildDocUtilUsers()
                        select p.Value).ToList();
            }
        }

        public static List<DocUtilDetails> GetDocs(IEnumerable<int> userIds)
        {
            using (var context = new OsbideProcs())
            {
                return (from p in context.GetBuildDocUtilBatch(string.Join(",", userIds.Where(x => x > 0)))
                        select new DocUtilDetails
                        {
                            LogId = p.Id,
                            UserId = p.SenderId,
                            EventDate = p.EventDate,
                            SolutionName = p.SolutionName,
                            FileName = p.FileName,
                            Document = p.Content,
                            BuildId = p.BuildId,
                            DocumentId = p.DocumentId,
                        })
                        .ToList();
            }
        }
    }
}
