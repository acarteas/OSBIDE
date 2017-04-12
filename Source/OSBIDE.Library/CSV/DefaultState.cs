using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Library.CSV
{
    //Default State
    public class DefaultState : CsvState
    {
        CsvDriver _CSVDriver;

        public DefaultState(CsvDriver CSVDriver)
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
                //Eat this quote, do not add it into current word
                _CSVDriver.SetStateToQuoteState();
            }
            else
            {
                _CSVDriver.AppendToCurrentCell(charTohandle);
            }
        }
    }
}