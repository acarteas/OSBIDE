using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Library.CSV
{
    //EndState
    public class EndState : CsvState
    {
        CsvDriver _CSVDriver;

        public EndState(CsvDriver CSVDriver)
        {
            _CSVDriver = CSVDriver;
        }

        public void Handle()
        {
            //End state does nothing. Let driver handle it from here.
        }
    }
}