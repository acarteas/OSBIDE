using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class BreakPoint : IComparable, IEquatable<BreakPoint>
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required(AllowEmptyStrings=true)]
        public string Condition { get; set; }

        [Required]
        public string File { get; set; }

        [Required]
        public int FileColumn { get; set; }

        [Required]
        public int FileLine { get; set; }

        [Required]
        public int FunctionColumnOffset { get; set; }

        [Required]
        public int FunctionLineOffset { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string FunctionName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool Enabled { get; set; }

        public BreakPoint()
        {
        }

        public BreakPoint(EnvDTE.Breakpoint bp)
        {
            this.Condition = bp.Condition;
            this.File = bp.File;
            this.FileColumn = bp.FileColumn;
            this.FileLine = bp.FileLine;
            this.FunctionColumnOffset = bp.FunctionColumnOffset;
            this.FunctionLineOffset = bp.FunctionLineOffset;
            this.FunctionName = bp.FunctionName;
            this.Name = bp.Name;
            this.Enabled = bp.Enabled;
        }

        public int CompareTo(object obj)
        {
            BreakPoint other = obj as BreakPoint;
            
            //same file?
            int result = File.CompareTo(other.File);
            if (result != 0)
            {
                return result;
            }
            
            //same column?
            result = FileColumn.CompareTo(other.FileColumn);
            if (result != 0)
            {
                return result;
            }

            //same line?
            result = FileLine.CompareTo(other.FileLine);
            if (result != 0)
            {
                return result;
            }

            //same file, same column, same line: must be same breakpoint
            return 0;
        }

        public bool Equals(BreakPoint other)
        {
            if (this.CompareTo(other) == 0)
            {
                return true;
            }
            return false;
        }
    }
}
