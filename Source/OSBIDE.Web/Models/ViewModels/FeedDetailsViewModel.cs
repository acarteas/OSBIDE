using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class FeedDetailsViewModel
    {
        public AggregateFeedItem FeedItem { get; set; }
        public bool IsSubscribed { get; set; }
        public string Ids { get; set; }
    }
}