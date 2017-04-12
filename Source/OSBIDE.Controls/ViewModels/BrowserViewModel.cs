using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Controls.ViewModels
{
    public class BrowserViewModel : ViewModelBase
    {
        private string _url = "";
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                OnPropertyChanged("Url");
            }
        }

        private string _authKey = "";
        public string AuthKey
        {
            get
            {
                return _authKey;
            }
            set
            {
                _authKey = value;
                OnPropertyChanged("AuthKey");
            }
        }


        
    }
}
