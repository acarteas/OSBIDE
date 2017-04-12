using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OSBIDE.Web.Models.FileSystem
{
    public class UserFilePath : FileSystemBase
    {
        private int _userId;
        private string _userPathPrefix = "Users";

        public UserFilePath(IFileSystem pathBuilder, int userId)
            : base(pathBuilder)
        {
            _userId = userId;
        }

        public override string GetPath()
        {
            string returnPath = Path.Combine(PathBuilder.GetPath(), _userPathPrefix, _userId.ToString());
            return returnPath;
        }
    }
}
