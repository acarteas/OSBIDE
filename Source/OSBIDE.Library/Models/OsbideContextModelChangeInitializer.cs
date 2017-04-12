using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class OsbideContextModelChangeInitializer : DropCreateDatabaseIfModelChanges<OsbideContext>
    {
        protected override void Seed(OsbideContext context)
        {
            base.Seed(context);
            OsbideContextSeeder.Seed(context);
        }
    }
}
