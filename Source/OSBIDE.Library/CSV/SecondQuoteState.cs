using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Library.CSV
{
    //SecondQuoteState
    public class SecondQuoteState : CsvState
    {
        CsvDriver _CSVDriver;

        public SecondQuoteState(CsvDriver CSVDriver)
        {
            _CSVDriver = CSVDriver;
        }

        public void Handle()
        {
            char charTohandle = _CSVDriver.GetNextCharacter();

            if (charTohandle == ',')
            {
                _CSVDriver.SetStateToEndState();
            }
            else if (charTohandle == '"')
            {
                //Add this quote, this is the only place quotes are wrote into the string
                _CSVDriver.AppendToCurrentCell(charTohandle);
                _CSVDriver.SetStateToQuoteState();
            }
            else
            {
                _CSVDriver.AppendToCurrentCell(charTohandle);
                _CSVDriver.SetStateToDefaultState();
            }
        }
    }

    
}