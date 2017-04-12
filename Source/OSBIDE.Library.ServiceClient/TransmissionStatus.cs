using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel;

namespace OSBIDE.Library.ServiceClient
{
    public class TransmissionStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private bool _isActive = false;
        private EventLog _currentTransmission = new EventLog();
        private EventLog _lastTransmission = new EventLog();
        private int _numberOfTransmissions = 0;
        private int _completedTransmissions = 0;
        private DateTime _lastTransmissionTime = DateTime.MinValue;


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }
        public EventLog CurrentTransmission
        {
            get
            {
                return _currentTransmission;
            }
            set
            {
                _currentTransmission = value;
                OnPropertyChanged("CurrentTransmission");
            }
        }
        public EventLog LastTransmission
        {
            get
            {
                return _lastTransmission;
            }
            set
            {
                _lastTransmission = value;
                OnPropertyChanged("LastTransmission");
            }
        }
        public int NumberOfTransmissions
        {
            get
            {
                return _numberOfTransmissions;
            }
            set
            {
                _numberOfTransmissions = value;
                OnPropertyChanged("NumberOfTransmissions");
            }
        }
        public int CompletedTransmissions
        {
            get
            {
                return _completedTransmissions;
            }
            set
            {
                _completedTransmissions = value;
                OnPropertyChanged("CompletedTransmissions");
            }
        }
        public DateTime LastTransmissionTime
        {
            get
            {
                return _lastTransmissionTime;
            }
            set
            {
                _lastTransmissionTime = value;
                OnPropertyChanged("LastTransmissionTime");
            }
        }
    }
}
