using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable()]
    public class ActionRequestLog
    {
        public const string ACTION_PARAMETER_DELIMITER = "|||";
        public const string ACTION_PARAMETER_NULL_VALUE = "[null]";

        [NonSerialized]
        private OsbideUser _creator;

        [Key]
        public int Id { get; set; }

        public int CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public OsbideUser Creator
        {
            get
            {
                return _creator;
            }
            set
            {
                _creator = value;
            }
        }

        public string ActionName { get; set; }

        public string ActionParameters { get; set; }

        public string ControllerName { get; set; }

        public string IpAddress { get; set; }

        public DateTime AccessDate { get; set; }

        public ActionRequestLog()
        {
            AccessDate = DateTime.UtcNow;
            Creator = new OsbideUser();
        }

        public ActionRequestLog(ActionRequestLog other)
        {
            if(Creator != null)
            {
                Creator = new OsbideUser(other.Creator);
            }
            ActionName = other.ActionName;
            ActionParameters = other.ActionParameters;
            ControllerName = other.ControllerName;
            IpAddress = other.IpAddress;
            AccessDate = other.AccessDate;
        }
    }
}
