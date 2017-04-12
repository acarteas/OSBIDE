using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library;
using System.Threading;
using System.Runtime.Caching;
using System.Data.Entity.Validation;
using System.ComponentModel;
using OSBIDE.Library.Logging;
using System.Threading.Tasks;
using OSBIDE.Library.ServiceClient.OsbideWebService;
using System.IO;

namespace OSBIDE.Library.ServiceClient
{
    public class ServiceClient : INotifyPropertyChanged
    {
        private static ServiceClient _instance;

        #region instance variables
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event EventHandler ReceivedNewSocialActivity = delegate { };
        private OsbideWebServiceClient _webServiceClient = null;
        private EventHandlerBase _events;
        private List<EventLog> _pendingLogs = new List<EventLog>();
        private ILogger _logger;
        private ObjectCache _cache = new FileCache(StringConstants.LocalCacheDirectory, new LibraryBinder());
        private Task _eventLogTask;
        private Task _sendLocalErrorsTask;
        private Task _checkStatusTask;
        private string _cacheRegion = "ServiceClient";
        private string _cacheKey = "logs";
        private bool _isSendingData = false;
        private bool _isCollectingDate = true;
        private TransmissionStatus _sendStatus = new TransmissionStatus();

        #endregion

        #region properties

        public bool IsCollectingData
        {
            get
            {
                lock (this)
                {
                    return _isCollectingDate;
                }
            }
            set
            {
                lock (this)
                {
                    _isCollectingDate = value;
                }
                OnPropertyChanged("IsCollectingData");
            }
        }

        public bool IsSendingData
        {
            get
            {
                lock (this)
                {
                    return _isSendingData;
                }
            }
            private set
            {
                lock (this)
                {
                    _isSendingData = value;
                }
                OnPropertyChanged("IsSendingData");
            }
        }

        public TransmissionStatus SendStatus
        {
            get
            {
                lock (this)
                {
                    return _sendStatus;
                }
            }
        }
        
        #endregion

        #region constructor

        private ServiceClient(EventHandlerBase dteEventHandler, ILogger logger)
        {
            _events = dteEventHandler;
            this._logger = logger;
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);

            //AC: "events" ends up being null during unit testing.  Otherwise, it should never happen.
            if (_events != null)
            {
                _events.EventCreated += new EventHandler<EventCreatedArgs>(OsbideEventCreated);
            }

            //if we don't have a cache record of pending logs when we start, create a dummy list
            if (!_cache.Contains(_cacheKey, _cacheRegion))
            {
                SaveLogsToCache(new List<EventLog>());
            }
            
        }

        #endregion

        #region public methods

        /// <summary>
        /// Returns a singleton instance of <see cref="ServiceClient"/>.  Unlike a normal
        /// singleton pattern, this will return NULL if GetInstance(EventHandlerBase dteEventHandler, ILogger logger)
        /// was not called first.
        /// </summary>
        /// <returns></returns>
        public static ServiceClient GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Returns a singleton instance of <see cref="ServiceClient"/>.  Parameters are only 
        /// used during the first instantiation of the <see cref="ServiceClient"/>.
        /// </summary>
        /// <param name="dteEventHandler"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ServiceClient GetInstance(EventHandlerBase dteEventHandler, ILogger logger)
        {
            if (_instance == null)
            {
                _instance = new ServiceClient(dteEventHandler, logger);
            }
            return _instance;
        }

        /// <summary>
        /// Will stop data from being sent to the server
        /// </summary>
        public void StopSending()
        {
            IsSendingData = false;
        }

        /// <summary>
        /// Begins sending data to the server
        /// </summary>
        public void StartSending()
        {
            bool wasNotSending = true;
            if (IsSendingData == true)
            {
                wasNotSending = false;
            }
            IsSendingData = true;

            //only start the tasks if we were not sending in the first place.
            if (wasNotSending == true)
            {
                //send off saved local errors
                _sendLocalErrorsTask = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            SendLocalErrorLogs();
                        }
                        catch (Exception ex)
                        {
                            _logger.WriteToLog("Error sending local logs to server: " + ex.Message, LogPriority.MediumPriority);
                        }
                    }
                    );

                //register a thread to keep our service key from going stale
                _checkStatusTask = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            CheckStatus();
                        }
                        catch (Exception ex)
                        {
                            _logger.WriteToLog("Error in CheckKey: " + ex.Message, LogPriority.MediumPriority);
                        }
                    }
                    );
            }
        }

        #endregion

        #region private send methods

        /// <summary>
        /// This function is responsible for continually asking for status updates from OSBIDE
        /// </summary>
        private void CheckStatus()
        {
            while (IsSendingData == true)
            {
                //this block checks to make sure that our authentication key is up to date
                string webServiceKey = "";
                lock (_cache)
                {
                    webServiceKey = _cache[StringConstants.AuthenticationCacheKey] as string;

                    bool result = false;
                    try
                    {
                        result = _webServiceClient.IsValidKey(webServiceKey);
                    }
                    catch (Exception)
                    {
                    }

                    //if result is false, our key has gone stale.  Try to login again
                    if (result == false)
                    {
                        string userName = _cache[StringConstants.UserNameCacheKey] as string;
                        byte[] passwordBytes = _cache[StringConstants.PasswordCacheKey] as byte[];
                        byte[] encoderKey = _cache[StringConstants.AesKeyCacheKey] as byte[];
                        byte[] encoderVector = _cache[StringConstants.AesVectorCacheKey] as byte[];
                        string password = "";
                        try
                        {
                            password = AesEncryption.DecryptStringFromBytes_Aes(passwordBytes, encoderKey, encoderVector);
                        }
                        catch (Exception)
                        {
                        }
                        if (userName != null && password != null)
                        {
                            webServiceKey = _webServiceClient.Login(userName, UserPassword.EncryptPassword(password, userName));
                            _cache[StringConstants.AuthenticationCacheKey] = webServiceKey;
                        }
                        else
                        {
                            IsSendingData = false;
                        }
                    }
                }

                //this block checks for recent user profile activity
                DateTime lastLocalProfileUpdate = DateTime.MinValue;
                string lastProfileActivityKey = "LastProfileActivity";
                
                //get last cached value
                lock (_cache)
                {
                    if (_cache.Contains(lastProfileActivityKey) == false)
                    {
                        _cache[lastProfileActivityKey] = DateTime.MinValue;
                    }
                    try
                    {
                        lastLocalProfileUpdate = (DateTime)_cache[lastProfileActivityKey];
                    }
                    catch (Exception)
                    {
                        lastLocalProfileUpdate = DateTime.MinValue;
                        _cache.Remove(lastProfileActivityKey);
                    }
                }

                //get last server value
                if(IsSendingData == true)
                {
                    DateTime lastServerProfileUpdate = DateTime.MinValue;
                    try
                    {
                        lastServerProfileUpdate = _webServiceClient.GetMostRecentSocialActivity(webServiceKey);
                    }
                    catch (Exception)
                    {
                        lastServerProfileUpdate = DateTime.MinValue;
                    }

                    if (lastLocalProfileUpdate < lastServerProfileUpdate)
                    {
                        //notify client of new social activity
                        if (ReceivedNewSocialActivity != null)
                        {
                            ReceivedNewSocialActivity(this, EventArgs.Empty);
                        }
                        _cache[lastProfileActivityKey] = lastServerProfileUpdate;
                    }
                }

                Thread.Sleep(new TimeSpan(0, 0, 3, 0, 0));
            }
        }

        private void SendLocalErrorLogs()
        {
            string dataRoot = StringConstants.DataRoot;
            string logExtension = StringConstants.LocalErrorLogExtension;
            string today = StringConstants.LocalErrorLogFileName;

            //find all log files
            string[] files = Directory.GetFiles(dataRoot);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == logExtension)
                {
                    //ignore today's log
                    if (Path.GetFileNameWithoutExtension(file) != today)
                    {
                        LocalErrorLog log = LocalErrorLog.FromFile(file);
                        int result = 0;
                        lock (_cache)
                        {
                            string webServiceKey = _cache[StringConstants.AuthenticationCacheKey] as string;
                            result = _webServiceClient.SubmitLocalErrorLog(log, webServiceKey);
                        }

                        //remove if file successfully sent
                        if (result != -1)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
        }

        private void SendError(Exception ex)
        {
            _logger.WriteToLog(string.Format("Push error: {0}", ex.Message), LogPriority.HighPriority);
            IsSendingData = false;
        }

        private void SendLogToServer(object data)
        {
            //only accept eventlog data
            if (!(data is EventLog))
            {
                return;
            }
            SendStatus.IsActive = true;

            //cast generic data to what we actually need
            EventLog newLog = data as EventLog;

            //find all logs that haven't been handled (submitted)
            List<EventLog> logsToBeSaved = null;

            //request exclusive access to our cache of existing logs
            lock (_cache)
            {
                //get pending records
                logsToBeSaved = GetLogsFromCache();

                //add new log to list
                logsToBeSaved.Add(newLog);

                //clear out cache
                SaveLogsToCache(new List<EventLog>());
            }

            //reorder by date received (created in our case)
            logsToBeSaved = logsToBeSaved.OrderBy(l => l.DateReceived).ToList();

            //loop through each log to be saved, give a dummy ID number
            int counter = 1;
            foreach (EventLog log in logsToBeSaved)
            {
                log.Id = counter;
                counter++;
            }

            //update our send status with the number of logs that we
            //plan to submit
            SendStatus.NumberOfTransmissions = logsToBeSaved.Count;

            //will hold the list of saved logs
            List<int> savedLogs = new List<int>(logsToBeSaved.Count);

            //send logs to the server
            foreach (EventLog log in logsToBeSaved)
            {
                //reset the log's sending user just to be safe
                _logger.WriteToLog(string.Format("Sending log with ID {0} to the server", log.Id), LogPriority.LowPriority);
                SendStatus.CurrentTransmission = log;

                try
                {
                    //the number that comes back from the web service is the log's local ID number.  Save
                    //for later when we clean up our local db.
                    int result = -1;
                    lock (_cache)
                    {
                        string webServiceKey = _cache[StringConstants.AuthenticationCacheKey] as string;
                        result = _webServiceClient.SubmitLog(log, webServiceKey);
                    }
                    savedLogs.Add(result);

                    //update our submission status
                    SendStatus.LastTransmissionTime = DateTime.UtcNow;
                    SendStatus.LastTransmission = log;
                    SendStatus.CompletedTransmissions++;
                }
                catch (Exception ex)
                {
                    SendError(ex);
                    break;
                }
            }

            //any logs that weren't saved successfully get added back into the cache
            foreach (int logId in savedLogs)
            {
                EventLog log = logsToBeSaved.Where(l => l.Id == logId).FirstOrDefault();
                if (log != null)
                {
                    logsToBeSaved.Remove(log);
                }
            }

            //save the modified list back into the cache
            lock (_cache)
            {
                SaveLogsToCache(logsToBeSaved);
            }
            SendStatus.IsActive = false;
        }

        /// <summary>
        /// Called whenever OSBIDE detects an event change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OsbideEventCreated(object sender, EventCreatedArgs e)
        {
            //create a new event log...
            EventLog eventLog = new EventLog(e.OsbideEvent);
            SendStatus.IsActive = false;

            //if the system is allowing web pushes, send it off.  Otherwise,
            //save to cache and try again later
            if (IsSendingData)
            {
                Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            this.SendLogToServer(eventLog);
                        }
                        catch (Exception ex)
                        {
                            _logger.WriteToLog(string.Format("SendToServer Error: {0}", ex.Message), LogPriority.HighPriority);
                        }
                    }
                    );
            }
            else
            {
                //only continue if we're okay to collect data
                if (IsCollectingData == false)
                {
                    return;
                }

                SendStatus.IsActive = false;
                lock (_cache)
                {
                    List<EventLog> cachedLogs = GetLogsFromCache();
                    cachedLogs.Add(eventLog);
                    SaveLogsToCache(cachedLogs);
                }
            }
        }

        #endregion

        #region private helpers


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveLogsToCache(List<EventLog> logs)
        {
            _cache.Set(_cacheKey, logs.ToArray(), new CacheItemPolicy(), _cacheRegion);
        }

        private List<EventLog> GetLogsFromCache()
        {
            List<EventLog> logs = new List<EventLog>();
            //get pending records
            try
            {
                logs = ((EventLog[])_cache.Get(_cacheKey, _cacheRegion)).ToList();
            }
            catch (Exception ex)
            {
                //saved logs corrupted, start over
                SaveLogsToCache(new List<EventLog>());
                logs = new List<EventLog>();
                _logger.WriteToLog(string.Format("GetLogsFromCache() error: {0}", ex.Message), LogPriority.HighPriority);
            }
            return logs;
        }
        #endregion
    }
}
