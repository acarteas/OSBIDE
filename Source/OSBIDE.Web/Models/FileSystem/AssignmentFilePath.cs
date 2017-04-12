using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.FileSystem
{
    public class AssignmentFilePath : FileSystemBase
    {
        private int _assignmentId;
        private string _assignmentPathPrefix = "Assignments";

        public AssignmentFilePath(IFileSystem pathBuilder, int id)
            : base(pathBuilder)
        {
            _assignmentId = id;
        }

        public IFileSystem Submission(int userId)
        {
            SubmissionFilePath sfp = new SubmissionFilePath(this, userId);
            return sfp;
        }

        public IFileSystem Submission(OsbideUser user)
        {
            return Submission(user.Id);
        }
        
        public IFileSystem Attachments()
        {
            return Directory("Attachments");
        }

        public override string GetPath()
        {
            string returnPath = Path.Combine(PathBuilder.GetPath(), _assignmentPathPrefix, _assignmentId.ToString());
            return returnPath;
        }
    }
}
