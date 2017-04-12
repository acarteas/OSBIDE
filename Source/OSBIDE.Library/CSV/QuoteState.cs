using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Library.CSV
{
    //QuoteState
    public class QuoteState : CsvState
    {
        CsvDriver _CSVDriver;

        public QuoteState(CsvDriver CSVDriver)
        {
            _CSVDriver = CSVDriver;
        }

        public void Handle()
        {
            char charTohandle = _CSVDriver.GetNextCharacter();


            if (charTohandle == '"')
            {
                //Eat this quote, do not add it into current word
                _CSVDriver.SetStateToSecondQuoteState();
            }
            else
            {
                _CSVDriver.AppendToCurrentCell(charTohandle);
            }
        }
    }
}