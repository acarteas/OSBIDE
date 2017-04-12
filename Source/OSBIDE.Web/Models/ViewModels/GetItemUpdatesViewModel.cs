using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class GetItemUpdatesViewModel
    {
        public int LogId { get; set; }
        public long LastPollTick { get; set; }
    }
}