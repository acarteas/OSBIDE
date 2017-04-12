using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace OSBIDE.Library.CSV
{

    public class CsvReader : IDisposable
    {
        private CsvDriver _CSVDriver;

        public CsvReader(Stream CSVStream)
        {
            _CSVDriver = new CsvDriver(CSVStream);
        }

        /// <summary>
        /// Parses the CSV into a List of List of strings. Where each List of strings represents 1 row."
        /// </summary>
        public List<List<string>> Parse()
        {
            return _CSVDriver.Drive();
        }

        public void Dispose()
        {
            _CSVDriver.Dispose();
        }
    }

    

    

    

    

    

}