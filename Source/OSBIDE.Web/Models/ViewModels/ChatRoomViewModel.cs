using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class ChatRoomViewModel
    {
        public List<ChatMessage> Messages { get; set; }
        public ChatRoom ActiveRoom { get; set; }
        public List<ChatRoom> Rooms { get; set; }
        public List<ChatRoomUserViewModel> Users { get; set; }
        public DateTime InitialDocumentDate { get; set; }
        

        public ChatRoomViewModel()
        {
            Rooms = new List<ChatRoom>();
            Users = new List<ChatRoomUserViewModel>();
            Messages = new List<ChatMessage>();
        }
    }
}