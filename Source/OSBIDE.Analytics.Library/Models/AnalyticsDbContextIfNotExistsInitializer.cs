using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class AnalyticsDbContextIfNotExistsInitializer : DropCreateDatabaseIfModelChanges<AnalyticsDbContext>
    {
        protected override void Seed(AnalyticsDbContext context)
        {
            base.Seed(context);
            AnalyticsDbContextSeeder.Seed(context);
        }
    }
}
