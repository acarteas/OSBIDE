using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class ErrorListItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int Column { get; set; }

        [Required]
        public int Line { get; set; }

        [Required]
        public string File { get; set; }

        [Required]
        public string Project { get; set; }

        [Required]
        public string Description { get; set; }

        [NonSerialized]
        private string _criticalErrorName = null;

        [NotMapped]
        public string CriticalErrorName
        {
            get
            {
                //storing result prevents multiple regex matches (should speed up execution time)
                if (_criticalErrorName == null)
                {
                    string pattern = "error ([^:]+)";
                    Match match = Regex.Match(Description, pattern);

                    //ignore bad matches
                    if (match.Groups.Count == 2)
                    {
                        _criticalErrorName = match.Groups[1].Value.ToLower().Trim();
                    }
                    else
                    {
                        _criticalErrorName = "";
                    }
                }
                return _criticalErrorName;
            }
        }

        public static ErrorListItem FromErrorItem(ErrorItem item)
        {
            ErrorListItem eli = new ErrorListItem();

            //Sometimes ErrorItem references are invalid.  Not sure why.
            try
            {
                eli.Project = item.Project;
                eli.Column = item.Column;
                eli.Line = item.Line;
                eli.File = item.FileName;
                eli.Description = item.Description;
            }
            catch (Exception)
            {
                
            }

            return eli;
        }
    }
}
