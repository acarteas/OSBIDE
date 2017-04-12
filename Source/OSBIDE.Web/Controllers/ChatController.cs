using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [RequiresVisualStudioConnectionForStudents]

    //Chat controller turned off for now
    [DenyAccess]
    public class ChatController : ControllerBase
    {
        private TimeSpan CHAT_TIMEOUT = new TimeSpan(0, 0, 7);
        public ActionResult Index(int id = -1)
        {
            if (id == -1)
            {
                //find the default chat room id
                id = Db.ChatRooms.Where(r => r.SchoolId == CurrentUser.SchoolId)
                    .Where(r => r.IsDefaultRoom == true)
                    .FirstOrDefault()
                    .Id;
            }

            //remove the user from all other rooms by setting last active date to a long time ago
            List<ChatRoomUser> records = Db.ChatRoomUsers.Where(u => u.UserId == CurrentUser.Id).Where(u => u.RoomId != id).ToList();
            foreach (ChatRoomUser record in records)
            {
                record.LastActivity = DateTime.UtcNow.AddDays(-1);
            }

            //Add a chat message indicating that the user has entered the room
            ChatRoomUser userRecord = Db.ChatRoomUsers.Where(u => u.UserId == CurrentUser.Id).Where(u => u.RoomId == id).FirstOrDefault();
            if (userRecord != null)
            {
                if (userRecord.LastActivity < DateTime.UtcNow.Subtract(CHAT_TIMEOUT))
                {
                    ChatMessage message = new ChatMessage()
                    {
                        AuthorId = CurrentUser.Id,
                        Message = string.Format("{0} has entered the chat room.", CurrentUser.FirstName),
                        MessageDate = DateTime.UtcNow,
                        RoomId = id
                    };
                    Db.ChatMessages.Add(message);
                }
            }
            Db.SaveChanges();

            ChatRoomViewModel vm = BuildViewModel(id);
            return View(vm);
        }

        private ChatRoomViewModel BuildViewModel(int activeRoomId)
        {
            //log the user into the database
            ChatRoomUser cru = Db.ChatRoomUsers
                .Where(u => u.UserId == CurrentUser.Id)
                .Where(u => u.RoomId == activeRoomId)
                .FirstOrDefault();

            if (cru == null)
            {
                cru = new ChatRoomUser()
                {
                    RoomId = activeRoomId,
                    UserId = CurrentUser.Id
                };
                Db.ChatRoomUsers.Add(cru);
            }
            cru.LastActivity = DateTime.UtcNow;
            Db.SaveChanges();

            //get all chat messages that are associated with the requested room and have been issued within the last hour
            List<ChatMessage> chatMessages = (from message in Db.ChatMessages
                                  .Include("Author")
                                              where message.RoomId == activeRoomId
                                              orderby message.MessageDate descending
                                              select message).Take(25).ToList();
            ChatRoom room = Db.ChatRooms.Where(r => r.Id == activeRoomId).FirstOrDefault();
            ChatRoomViewModel vm = new ChatRoomViewModel();
            
            //find the last date that we're returning
            DateTime minDate = DateTime.MinValue.AddDays(1);
            if(chatMessages.Count > 0)
            {
                minDate = chatMessages.LastOrDefault().MessageDate;
            }

            vm.Messages = chatMessages;
            vm.ActiveRoom = room;
            vm.InitialDocumentDate = minDate;
            vm.Rooms = Db.ChatRooms.Where(r => r.SchoolId == CurrentUser.SchoolId).ToList();
            vm.Users = new List<ChatRoomUserViewModel>();

            DateTime minActivityDate = DateTime.UtcNow.Subtract(CHAT_TIMEOUT);
            var roomUsers = from chatUser in Db.ChatRoomUsers
                            where chatUser.User.SchoolId == CurrentUser.SchoolId
                            && chatUser.RoomId == activeRoomId
                            && chatUser.LastActivity > minActivityDate
                            select chatUser.UserId;
            Dictionary<int, int> activeUsers = new Dictionary<int, int>();
            foreach (int user in roomUsers)
            {
                activeUsers[user] = user;
            }
            foreach(OsbideUser user in Db.Users.Where(u => u.SchoolId == CurrentUser.SchoolId).OrderBy(u => u.FirstName))
            {
                ChatRoomUserViewModel cvm = new ChatRoomUserViewModel(user);
                if (activeUsers.ContainsKey(user.Id))
                {
                    cvm.IsCssVisible = true;
                }

                UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
                string url = u.Action("Picture", "Profile", new { id = cvm.Id, size = 24 });
                cvm.ProfileImageUrl = url;

                vm.Users.Add(cvm);
            }

            return vm;
        }

        [HttpGet]
        public ActionResult OldMessages(int chatRoomId, long firstMessageTick, int count)
        {
            DateTime firstMessage = new DateTime(firstMessageTick);
            List<ChatMessage> messages = new List<ChatMessage>();
            messages = (from message in Db.ChatMessages
                                  .Include("Author")
                              where message.RoomId == chatRoomId
                              && message.MessageDate < firstMessage
                              select message).ToList();
            ChatRoom room = Db.ChatRooms.Where(r => r.Id == chatRoomId).FirstOrDefault();
            ChatRoomViewModel vm = new ChatRoomViewModel()
            {
                Messages = messages,
                ActiveRoom = room
            };
            return View("RecentMessages", vm);
        }

        [HttpGet]
        public ActionResult RecentMessages(int chatRoomId, long lastMessageTick)
        {
            int timeout = 40;
            int timeoutCounter = 0;
            TimeSpan timeBetweenQueries = new TimeSpan(0, 0, 0, 1);
            bool hasRequestTimedOut = false;
            DateTime lastMessage = new DateTime(lastMessageTick);
            List<ChatMessage> recentMessages = new List<ChatMessage>();

            while (recentMessages.Count == 0 && hasRequestTimedOut == false)
            {
                recentMessages = (from message in Db.ChatMessages
                                  .Include("Author")
                                  where message.RoomId == chatRoomId
                                  && message.MessageDate > lastMessage
                                  select message).ToList();
                if (recentMessages.Count == 0)
                {
                    Thread.Sleep(timeBetweenQueries);
                    timeoutCounter++;
                    if (timeoutCounter == timeout)
                    {
                        hasRequestTimedOut = true;
                    }
                }
            }
            ChatRoom room = Db.ChatRooms.Where(r => r.Id == chatRoomId).FirstOrDefault();
            ChatRoomViewModel vm = new ChatRoomViewModel()
            {
                Messages = recentMessages,
                ActiveRoom = room
            };
            return View(vm);
        }

        public JsonResult RoomUsers()
        {
            const string chatRoomIdKey = "ChatRoomId";
            int chatRoomId = 0;
            int timeout = 20;
            int timeoutCounter = 0;
            TimeSpan timeBetweenQueries = new TimeSpan(0, 0, 0, 2);
            bool hasRequestTimedOut = false;

            Int32.TryParse(Request[chatRoomIdKey], out chatRoomId);

            //turn JSON data into objects
            List<ChatRoomUserViewModel> users = new List<ChatRoomUserViewModel>();
            foreach (string key in Request.Form.AllKeys)
            {
                //ignore chat room key
                if (key.CompareTo(chatRoomIdKey) != 0)
                {
                    int userId = 0;
                    if (Int32.TryParse(key, out userId))
                    {
                        users.Add(new ChatRoomUserViewModel()
                        {
                            CssClasses = Request[key],
                            Id = userId
                        });
                    }
                }
            }

            Dictionary<int, int> mergedIdList = new Dictionary<int, int>();
            Dictionary<int, int> originalIds = new Dictionary<int, int>();
            ChatRoomViewModel updatedModel = null;

            //capture all users that were active at the time the HTTP request was made
            foreach (ChatRoomUserViewModel user in users)
            {
                //only add visible users
                if (user.IsCssVisible)
                {
                    originalIds.Add(user.Id, user.Id);
                }
            }

            
            while (hasRequestTimedOut == false)
            {
                updatedModel = BuildViewModel(chatRoomId);

                //Capture all users that are currently active
                Dictionary<int, int> updatedIds = new Dictionary<int, int>();
                foreach (ChatRoomUserViewModel user in updatedModel.Users)
                {
                    if (user.IsCssVisible == true)
                    {
                        updatedIds.Add(user.Id, user.Id);
                    }
                }

                //do the easy diff check first
                if (originalIds.Count != updatedIds.Count)
                {
                    hasRequestTimedOut = true;
                }
                else
                {

                    //merge the list of active and inactive users into a single list
                    mergedIdList = new Dictionary<int, int>();
                    foreach (int key in originalIds.Keys)
                    {
                        if (mergedIdList.ContainsKey(key) == false)
                        {
                            mergedIdList.Add(key, key);
                        }
                    }
                    foreach (int key in updatedIds.Keys)
                    {
                        if (mergedIdList.ContainsKey(key) == false)
                        {
                            mergedIdList.Add(key, key);
                        }
                    }

                    //now, loop through the list of merged keys to find any changes
                    foreach (int key in mergedIdList.Keys)
                    {
                        //If the merged list differs from either the original or updated, that means that 
                        //at least one user has left or joined the chat room since the original request was made.
                        //In this case, we need to send back the updated list to the client immediately.
                        if (originalIds.ContainsKey(key) == false || updatedIds.ContainsKey(key) == false)
                        {
                            hasRequestTimedOut = true;
                            break;
                        }
                    }
                }

                //check for a general timeout
                if (hasRequestTimedOut == false)
                {
                    Thread.Sleep(timeBetweenQueries);
                    timeoutCounter++;
                    if (timeoutCounter == timeout)
                    {
                        hasRequestTimedOut = true;
                    }
                }
            }

            var simpleUsers = updatedModel.Users.Select(u => new
            {
                Id = u.Id,
                CssClasses = u.CssClasses
            });

            return this.Json(simpleUsers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostMessage(int chatRoomId, string message)
        {
            var result = PostMessageAsync(chatRoomId, message);
            return RedirectToAction("Index", new { id = chatRoomId });
        }

        [HttpPost]
        public JsonResult PostMessageAsync(int chatRoomId, string message)
        {
            int messageId = -1;
            DateTime messageTimestamp = DateTime.MinValue;
            var result = new { id = -1, timestamp = -1 };
            if (Db.ChatRooms.Where(r => r.Id == chatRoomId).Count() > 0)
            {
                if (string.IsNullOrEmpty(message) == false)
                {
                    ChatMessage chat = new ChatMessage()
                    {
                        AuthorId = CurrentUser.Id,
                        Message = message,
                        MessageDate = DateTime.UtcNow,
                        RoomId = chatRoomId
                    };
                    Db.ChatMessages.Add(chat);
                    Db.SaveChanges();
                    messageId = chat.Id;
                    messageTimestamp = chat.MessageDate;
                }
            }
            return this.Json(new { id = messageId, timestamp = messageTimestamp.Ticks });
        }
    }
}
