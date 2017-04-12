using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

//the following is a diagram of our file system.  Items in brackets [] indicate
//using a key of sorts (e.g. the user id).  Items in curly braces {} indicate
//the intended use of the folder
/*
 *
 *                          ____FileSystem_____
 *                         /                   \  
 *                        /                     \  
 *                    Courses                  Users                                 
 *                     /                          \                                 
 *                [courseId]                    [userId]
 *               /     |   \_________               \
 *              /      |             \             {global user content}
 *     CourseDocs      |              \
 *          |    Assignments         Gradebook
 *          |          |                 |
 *  {course docs}      |        {gradebook.zip / gradebook file}
 *                     |  
 *                     |
 *               [AssignmentId]
 *               /     |        
 *              /      |         
 *      Submissions  Attachments   
 *         |           |  
 *         |           |
 *         |           |
 *      [UserId]   {assignment-specific files}    
 *         |
 *    {submission}      
 *                                     
 * */
namespace OSBIDE.Web.Models.FileSystem
{
    public class FileSystem : FileSystemBase
    {
        private string _fileSystemRoot = "";

        public FileSystem()
            : base(new EmptyFilePathDecorator())
        {
            _fileSystemRoot = HttpContext.Current.Server.MapPath("~\\App_Data\\FileSystem\\");
        }

        public FileSystem(string rootPath)
            : base(new EmptyFilePathDecorator())
        {
            _fileSystemRoot = rootPath;
        }

        public CourseFilePath Course(int id)
        {
            CourseFilePath cfp = new CourseFilePath(this, id);
            return cfp;
        }

        public CourseFilePath Course(Course course)
        {
            CourseFilePath cfp = new CourseFilePath(this, course.Id);
            return cfp;
        }

        public UserFilePath Users(int id)
        {
            UserFilePath ufp = new UserFilePath(this, id);
            return ufp;
        }

        public UserFilePath Users(OsbideUser user)
        {
            UserFilePath ufp = new UserFilePath(this, user.Id);
            return ufp;
        }

        public override string GetPath()
        {
            //FileSystem comes first
            return _fileSystemRoot;
        }
    }
}
