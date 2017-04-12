using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace OSBIDE.Library.Models
{
    class OsbideContextIfNotExistsInitializer : CreateDatabaseIfNotExists<OsbideContext>
    {
        protected override void Seed(OsbideContext context)
        {
            base.Seed(context);
            OsbideContextSeeder.Seed(context);
        }
    }
}
