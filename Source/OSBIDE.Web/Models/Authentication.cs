using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models
{
    public class Authentication
    {
        private FileCache _cache;
        private OsbideContext _db;
        private const string userNameKey = "UserName";
        private const string authKey = "AuthKey";

        public static string ProfileCookieKey
        {
            get
            {
                return "osble_profile";
            }
        }

        public Authentication()
        {
            //set up DB
            _db = OsbideContext.DefaultWebConnection;

            //set up cache
            _cache = FileCacheHelper.GetGlobalCacheInstance();
            _cache.DefaultRegion = "AuthenticationService";

            //have our cache kill things after 2 days
            _cache.DefaultPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(2, 0, 0, 0, 0) };
        }

        /// <summary>
        /// Returns true if the supplied key is valid
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public bool IsValidKey(string authToken)
        {
            int id = -1;
            try
            {
                id = (int)_cache[authToken];
            }
            catch (Exception)
            {
            }
            if (id < 0)
            {
                return false;
            }
            return true;
        }

        public string GetAuthenticationKey()
        {
            if (HttpContext.Current != null)
            {
                try
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(ProfileCookieKey);
                    string userKey = cookie.Values[userNameKey];
                    return userKey;
                }
                catch (Exception)
                {
                }
            }
            return "";
        }

        /// <summary>
        /// Generates a random string of the given lenght
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateRandomString(int length)
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < length; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the active user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public OsbideUser GetActiveUser(string authToken)
        {
            int id = -1;
            OsbideUser profile = null;
            try
            {
                id = (int)_cache[authToken];
                profile = _db.Users.Find(id);
            }
            catch(Exception)
            {
            }
            if (profile == null)
            {
                return new OsbideUser();
            }
            return new OsbideUser(profile);
        }

        public int GetActiveUserId(string authToken)
        {
            int id = -1;
            try
            {
                id = (int)_cache[authToken];
            }
            catch (Exception)
            {
            }
            return id;
        }

        /// <summary>
        /// Logs the user into the system
        /// </summary>
        /// <param name="profile"></param>
        public string LogIn(OsbideUser profile)
        {
            profile = new OsbideUser(profile);
            HttpCookie cookie = new HttpCookie(ProfileCookieKey);

            //compute hash for this login attempt
            string hash = ComputeHash(profile.Email);

            //store profile in the authentication hash
            _cache[hash] = profile.Id;

            //store the key to the hash inside a cookie for the user
            cookie.Values[userNameKey] = hash;

            //Set a really long expiration date for the cookie.  Note that the server's copy of the
            //hash key will expire much sooner than this.
            cookie.Expires = DateTime.UtcNow.AddDays(360);

            //and then store it in the next response
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies.Set(cookie);
            }

            return hash;
        }

        /// <summary>
        /// Logs the current user out of the system
        /// </summary>
        public void LogOut()
        {
            if (HttpContext.Current != null)
            {
                //cookie might not exist
                try
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(ProfileCookieKey);
                    cookie.Expires = DateTime.UtcNow.AddDays(-1d);
                    HttpContext.Current.Response.Cookies.Set(cookie);
                }
                catch (Exception)
                {
                    //ignore exception
                }
            }
        }

        /// <summary>
        /// Computes a hash of the given text.  Adds a bit of salt using the current date.
        /// </summary>
        /// <param name="hashText"></param>
        /// <returns></returns>
        public string ComputeHash(string text)
        {
            //build our string to hash
            string date = DateTime.UtcNow.ToLongTimeString();
            string hashString = text + date;

            //compute the hash
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                byte[] textBytes = Encoding.ASCII.GetBytes(hashString);
                byte[] hashBytes = sha1.ComputeHash(textBytes);
                string hashText = BitConverter.ToString(hashBytes);

                //return the hash to the caller
                return hashText;
            }
        }

    }
}