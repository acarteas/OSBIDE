using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    [DataContract]
    public class EventLog
    {
        [Key]
        [Required]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string LogType { get; set; }

        [Required]
        [DataMember]
        public int EventTypeId { get; set; }

        [Required]
        [DataMember]
        public DateTime DateReceived { get; set; }

        [Required]
        [DataMember]
        public virtual EventLogData Data { get; set; }

        [Required]
        [DataMember]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        [IgnoreDataMember]
        public virtual OsbideUser Sender { get; set; }

        [DataMember]
        public string AssemblyVersion { get; set; }

        [IgnoreDataMember]
        public virtual IList<LogCommentEvent> Comments
        {
            get
            {
                return _comments;
            }
            set
            {
                _comments = value;
            }
        }

        [IgnoreDataMember]
        [NonSerialized]
        private IList<LogCommentEvent> _comments;

        [IgnoreDataMember]
        [NonSerialized]
        private IList<EventLogSubscription> _subscriptions = new List<EventLogSubscription>();

        [IgnoreDataMember]
        public virtual IList<EventLogSubscription> Subscriptions
        {
            get
            {
                return _subscriptions;
            }
            set
            {
                _subscriptions = value;
            }
        }

        public EventLog()
        {
            DateReceived = DateTime.UtcNow;
            AssemblyVersion = OSBIDE.Library.StringConstants.LibraryVersion;
            Subscriptions = new List<EventLogSubscription>();
            Comments = new List<LogCommentEvent>();
            Data = new EventLogData();
        }

        public EventLog(IOsbideEvent evt, OsbideUser sender)
            : this(evt)
        {
            //were we sent a null user?
            if (sender.FirstName == null && sender.LastName == null)
            {
                //replace with a generic user
                sender = OsbideUser.GenericUser();
            }
            Sender = sender;
            if (sender.Id != 0)
            {
                SenderId = sender.Id;
            }
        }

        public EventLog(IOsbideEvent evt)
            : this()
        {
            DateReceived = DateTime.UtcNow;
            LogType = evt.EventName;
            Data = new EventLogData()
            {
                LogId = this.Id,
                BinaryData = EventFactory.ToZippedBinary(evt)
            };
        }

        public EventLog(EventLog copyLog) : this()
        {
            DateReceived = copyLog.DateReceived;
            Id = copyLog.Id;
            LogType = copyLog.LogType;
            Data = copyLog.Data;
            Sender = new OsbideUser(copyLog.Sender);
            SenderId = copyLog.SenderId;
            AssemblyVersion = copyLog.AssemblyVersion;
        }

    }
}
