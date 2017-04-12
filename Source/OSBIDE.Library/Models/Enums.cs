using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public static class Enums
    {
        public enum ServiceCode { 
                                  GenericError = -100,  
                                  DatabaseError = -99,
                                  AuthenticationError = -98,
                                  Ok = 1 
                                }
    }
}
