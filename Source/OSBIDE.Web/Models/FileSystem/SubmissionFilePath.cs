using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OSBIDE.Web.Models.FileSystem
{
    public class SubmissionFilePath : FileSystemBase
    {
        private int _userId;
        private string _submissionPrefix = "Submissions";

        public SubmissionFilePath(IFileSystem pathBuilder, int userId)
            : base(pathBuilder)
        {
            _userId = userId;
        }

        public override string GetPath()
        {
            string returnPath = Path.Combine(PathBuilder.GetPath(), _submissionPrefix, _userId.ToString());
            return returnPath;
        }
    }
}
