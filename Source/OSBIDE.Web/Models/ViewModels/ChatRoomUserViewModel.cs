using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class ChatRoomUserViewModel : OsbideUser
    {
        public string ProfileImageUrl { get; set; }

        public string CssClasses
        {
            get;
            set;
        }

        public bool IsCssVisible
        {
            get
            {
                if (CssClasses.Contains("room-user-inactive"))
                {
                    return false;
                }
                return true;
            }
            set
            {
                if(value == true)
                {
                    CssClasses = CssClasses.Replace("room-user-inactive", "");
                }
                else
                {
                    CssClasses += "room-user-inactive";
                }
            }
        }

        public ChatRoomUserViewModel()
            : this(new OsbideUser())
        {
        }

        public ChatRoomUserViewModel(string cssClasses)
            : base()
        {
            CssClasses = cssClasses;
        }


        public ChatRoomUserViewModel(OsbideUser user)
            : base(user)
        {
            CssClasses = "room-user room-user-inactive";
        }
    }
}