using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Data.DomainObjects
{
    public class FeedItemCreator
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DefaultCourseName { get; set; }
        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}", LastName, FirstName);
            }
        }
        public string FirstAndLastName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }
    }
}
