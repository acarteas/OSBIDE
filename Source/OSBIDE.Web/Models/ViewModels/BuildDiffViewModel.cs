using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class BuildDiffViewModel
    {
        public const string DIFF_ESCAPE = "|:|:|";
        public BuildEvent OriginalBuild { get; set; }
        public BuildEvent ModifiedBuild { get; set; }
        public int LineErrorMargin { get; set; }
        public int ActiveFileId { get; set; }
        public List<DocumentError> DocumentErrors { get; set; }
        public BuildDiffViewModel()
        {
            LineErrorMargin = 7;
            DocumentErrors = new List<DocumentError>();
        }

        /// <summary>
        /// Marks a line as having a build error
        /// </summary>
        /// <param name="error"></param>
        public void RecordError(DocumentError error)
        {
            if(DocumentErrors.Contains(error, new DocumentErrorComparer()) == false)
            {
                DocumentErrors.Add(error);
            }
        }

        /// <summary>
        /// Returns only lines with errors and their surrounding code for the original document.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<BuildDiffLine> GetOriginalDocumentLines(string file)
        {
            return GetDocumentLines(file, OriginalBuild.Documents.Where(f => f.Document.FileName == file).Select(f => f.Document).FirstOrDefault());
        }

        /// <summary>
        /// Returns only lines with errors and their surrounding code for the modified document.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<BuildDiffLine> GetModifiedDocumentLines(string file)
        {
            return GetDocumentLines(file, ModifiedBuild.Documents.Where(f => f.Document.FileName == file).Select(f => f.Document).FirstOrDefault());
        }

        private List<BuildDiffLine> GetDocumentLines(string file, CodeDocument document)
        {
            file = Path.GetFileName(file);
            List<BuildDiffLine> lines = new List<BuildDiffLine>();
            List<DocumentError> fileErrors = DocumentErrors.Where(d => d.FileName == file).ToList();
            List<int> linesSurroundingErrors = new List<int>();

            if (document == null)
            {
                return lines;
            }

            //first pass: build up all lines that are near errors
            foreach (DocumentError error in fileErrors)
            {
                for (int i = error.LineNumber - LineErrorMargin; i < error.LineNumber + LineErrorMargin; i++)
                {
                    if (linesSurroundingErrors.Contains(i) == false)
                    {
                        linesSurroundingErrors.Add(i);
                    }
                }
            }

            //second pass: connect lines with errors to error source and line content
            foreach (int lineNumber in linesSurroundingErrors)
            {
                string content = document.Lines.ElementAtOrDefault(lineNumber);
                DocumentError error = fileErrors.Where(e => e.LineNumber == lineNumber).FirstOrDefault();
                if (string.IsNullOrEmpty(content) == true)
                {
                    content = "";
                }
                if (error == null)
                {
                    error = new DocumentError()
                    {
                        FileName = file,
                        LineNumber = lineNumber,
                        Source = DocumentErrorSource.None
                    };
                }
                BuildDiffLine line = new BuildDiffLine()
                {
                    Content = content,
                    Error = error
                };
                lines.Add(line);
            }
            return lines;
        }
    }
}