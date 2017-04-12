using OSBIDE.Data.SQLDatabase.Edmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Data.SQLDatabase
{
    public class MarkReadProc
    {
        public static void Update(int? postid, int? userid, bool val)
        {
            using (var context = new OsbideProcs())
            {
                context.MarkRead(postid, userid, val);
            }
        }
    }
}
