using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class ServerLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime LogDate { get; set; }
        public string LogMessage { get; set; }

        public ServerLog()
        {
            LogDate = DateTime.UtcNow;
            LogMessage = "";
        }
    }
}
