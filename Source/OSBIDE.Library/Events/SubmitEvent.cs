using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using OSBIDE.Library.Models;
using Ionic.Zip;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class SubmitEvent : IOsbideEvent, IModelBuilderExtender
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        private string _solutionName = "";
        public string SolutionName
        {
            get
            {
                return _solutionName;
            }
            set
            {
                _solutionName = value;
            }
        }

        [Required]
        public string EventName { get { return SubmitEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "SubmitEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Submit Assignment"; } }

        [Required]
        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; }

        [Required]
        [Column(TypeName = "image")]
        public byte[] SolutionData { get; private set; }

        public SubmitEvent()
        {
            EventDate = DateTime.UtcNow;
            SolutionData = new byte[0];
        }

        public SubmitEvent(string solutionPath)
            : this()
        {
            SolutionName = solutionPath;
        }

        public SubmitEvent(DTE2 dte)
            : this(dte.Solution.FullName)
        {
        }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            SubmitEvent evt = new SubmitEvent();
            if (values.ContainsKey("Id"))
            {
                evt.Id = (int)values["Id"];
            }
            if (values.ContainsKey("EventLogId"))
            {
                evt.EventLogId = (int)values["EventLogId"];
            }
            if (values.ContainsKey("EventLog"))
            {
                evt.EventLog = (EventLog)values["EventLog"];
            }
            if (values.ContainsKey("EventDate"))
            {
                evt.EventDate = (DateTime)values["EventDate"];
            }
            if (values.ContainsKey("SolutionName"))
            {
                evt.SolutionName = values["SolutionName"].ToString();
            }
            if (values.ContainsKey("AssignmentName"))
            {
                evt.AssignmentId = (int)values["AssignmentId"];
            }
            if (values.ContainsKey("SolutionData"))
            {
                evt.SolutionData = values["SolutionData"] as byte[];
            }
            return evt;
        }

        /// <summary>
        /// Creates a binary of the current solution.
        /// </summary>
        public void CreateSolutionBinary()
        {
            MemoryStream stream = new MemoryStream();
            using (ZipFile zip = new ZipFile())
            {
                string rootPath = Path.GetDirectoryName(SolutionName);
                string[] files = GetSolutionFileList(rootPath);
                foreach (string file in files)
                {
                    string directoryName = Path.GetDirectoryName(file).Replace(rootPath, "");
                    zip.AddFile(file, directoryName);
                }
                zip.Save(stream);
                stream.Position = 0;
            }
            SolutionData = stream.ToArray();
        }

        /// <summary>
        /// Saves the binary of the submitted file.
        /// </summary>
        public void CreateSolutionBinary(byte[] fileData)
        {
            SolutionData = fileData;
        }

        /// <summary>
        /// Builds a list of solution files to be zipped.  This function skips 
        /// unnecessary files and folders.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string[] GetSolutionFileList(string path)
        {
            string[] noDirectorySearchList = { "bin", "obj", "debug", "release", "ipch", "packages" };
            string[] noFileExtension = { ".sdf", ".ipch", ".dll" };
            List<string> filesToAdd = new List<string>(10);

            foreach (string file in Directory.GetFiles(path))
            {
                //ignore file extensions flagged in our no extension list
                if (noFileExtension.Contains(Path.GetExtension(file).ToLower()))
                {
                    continue;
                }
                filesToAdd.Add(file);
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                //ignore directories flagged in our no search list
                string[] directoryPieces = directory.ToLower().Split(Path.DirectorySeparatorChar);
                string localDirectory = directoryPieces[directoryPieces.Length - 1];
                if (noDirectorySearchList.Contains(localDirectory))
                {
                    continue;
                }

                //add in subdirectory files
                filesToAdd = filesToAdd.Union(GetSolutionFileList(directory)).ToList();
            }
            return filesToAdd.ToArray();
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubmitEvent>()
                .HasRequired(s => s.EventLog)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
