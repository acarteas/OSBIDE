using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.FileSystem
{
    public class CourseFilePath : FileSystemBase
    {
        private int _courseID;
        private string _coursePathPrefix = "Courses";

        public CourseFilePath(IFileSystem pathBuilder, int courseID)
            : base(pathBuilder)
        {
            _courseID = courseID;
        }

        public AssignmentFilePath Assignment(int id)
        {
            AssignmentFilePath afp = new AssignmentFilePath(this, id);
            return afp;
        }

        public AssignmentFilePath Assignment(Assignment assignment)
        {
            AssignmentFilePath afp = new AssignmentFilePath(this, assignment.Id);
            return afp;
        }

        public IFileSystem CourseDocs()
        {
            return Directory("CourseDocs");
        }

        public GradebookFilePath Gradebook()
        {
            GradebookFilePath gfp = new GradebookFilePath(this);
            return gfp;
        }

        public override string GetPath()
        {
            string returnPath = Path.Combine(PathBuilder.GetPath(), _coursePathPrefix, _courseID.ToString());
            return returnPath;
        }
    }
}
