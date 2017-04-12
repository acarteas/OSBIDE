using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public class ContentCodingViewModel
    {
        private OsbideContext _db { get; set; }
        public List<CodedOsbideFeedItem> CodedOsbideItems { get; set; }
        public ContentCodingViewModel()
        {
            _db = OsbideContext.DefaultWebConnection;
            CodedOsbideItems = new List<CodedOsbideFeedItem>();
        }

        public void LoadCodedOsbideItems()
        {
            var query = from item in _db.CodedOsbideFeedItems
                        where item.Cat != null && item.Cat != "Reflection"
                        select item;
            List<CodedOsbideFeedItem> items = query.ToList();

            //due to inefficiencies in the original algorithm, some posts might actually come from
            //outside CptS 122, the target group for this analysis.  As such, we need to remove
            //these from consideration
            var usersQuery = from course in _db.Courses
                             join cur in _db.CourseUserRelationships on course.Id equals cur.CourseId
                             join user in _db.Users on cur.UserId equals user.Id
                             where course.Id == 4
                             select user;
            List<int> userIds = usersQuery.Select(u => u.Id).ToList();
            List<int> rejectedIds = new List<int>();
            List<CodedOsbideFeedItem> acceptedItems = new List<CodedOsbideFeedItem>();
            foreach (CodedOsbideFeedItem item in items)
            {
                //only look at top-level posts
                if(item.PostId.ToString().Contains('.') == false)
                {
                    //match found, reject post
                    if(userIds.Contains((int)item.Author))
                    {
                        rejectedIds.Add((int)item.PostId);
                    }
                }

                //add to accepted feed items if the id hasn't been rejected
                int idAsInt = (int)item.PostId;
                if(rejectedIds.Contains(idAsInt) == false)
                {
                    acceptedItems.Add(item);
                }
            }
            CodedOsbideItems = acceptedItems;
        }

        public Dictionary<string, Dictionary<string, int>> CategorizeItems()
        {
            Dictionary<string, Dictionary<string, int>> categories = new Dictionary<string, Dictionary<string, int>>();
            foreach (CodedOsbideFeedItem item in CodedOsbideItems)
            {
                if(categories.ContainsKey(item.Cat) == false)
                {
                    categories.Add(item.Cat, new Dictionary<string, int>());
                }
                if(item.SubCat == null)
                {
                    item.SubCat = "";
                }
                if(categories[item.Cat].ContainsKey(item.SubCat) == false)
                {
                    categories[item.Cat].Add(item.SubCat, 0);
                }
                categories[item.Cat][item.SubCat] += 1;
            }
            return categories;
        }
    }
}
