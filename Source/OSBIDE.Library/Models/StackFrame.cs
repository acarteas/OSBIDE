using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using EnvDTE;
using OSBIDE.Library.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class StackFrame : IModelBuilderExtender
    {
        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string FunctionName { get; set; }

        [Required(AllowEmptyStrings=true)]
        public string Module { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Language { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string FileName { get; set; }

        [Required]
        public int ExceptionEventId { get; set; }

        [Required(AllowEmptyStrings=true)]
        public string ReturnType { get; set; }

        [Required]
        public int LineNumber { get; set; }

        [ForeignKey("ExceptionEventId")]
        public virtual ExceptionEvent Exception { get; set; }

        /// <summary>
        /// The depth of the stack frame.  A depth of 0 means that it is the top
        /// most stack frame.
        /// </summary>
        [Required]
        public int Depth { get; set; }

        public virtual IList<StackFrameVariable> Variables { get; set; }

        public StackFrame()
        {
            Variables = new List<StackFrameVariable>();
        }

        public StackFrame(EnvDTE.StackFrame frame)
            : this()
        {
            EnvDTE90a.StackFrame2 frame2 = null;
            try
            {
                frame2 = (EnvDTE90a.StackFrame2)frame;
                this.LineNumber = (int)frame2.LineNumber;
                this.FileName = frame2.FileName;
            }
            catch (Exception)
            {
                this.LineNumber = 0;
                this.FileName = "";
            }
            this.FunctionName = frame.FunctionName;
            this.Module = frame.Module;
            this.Language = frame.Language;
            this.ReturnType = frame.ReturnType;
            foreach (Expression local in frame.Locals)
            {
                StackFrameVariable var = new StackFrameVariable()
                {
                    Name = local.Name,
                    Value = local.Value
                };
                this.Variables.Add(var);
            }
        }

        public void BuildRelationship(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StackFrame>()
                .HasMany(sf => sf.Variables)
                .WithRequired(v => v.StackFrame)
                .WillCascadeOnDelete(true);
        }
    }
}
