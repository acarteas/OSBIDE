using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.IO;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    [DataContract]
    public class LocalErrorLog
    {
        [Key]
        [Required]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        [DataMember]
        public OsbideUser Sender { get; set; }

        [Required]
        [DataMember]
        public DateTime LogDate { get; set; }

        [Required]
        [DataMember]
        public string Content { get; set; }

        public LocalErrorLog()
        {
            Content = "";
            LogDate = DateTime.MinValue;
        }

        public static LocalErrorLog FromFile(string filePath)
        {
            LocalErrorLog log = new LocalErrorLog();
            DateTime localDateTime = DateTime.MinValue;
            DateTime.TryParse(Path.GetFileNameWithoutExtension(filePath), out localDateTime);
            log.LogDate = localDateTime;
            try
            {
                log.Content = File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                log.Content = "";
            }
            return log;
        }
    }
}
