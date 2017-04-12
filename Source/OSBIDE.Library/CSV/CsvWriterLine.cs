using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.CSV
{
    public class CsvWriterLine
    {
        private List<string> lineItems = new List<string>();

        public void Add(string cell)
        {
            lineItems.Add(cell);
        }

        public void Add(string cell, int index)
        {
            this[index] = cell;
        }
        

        public string this[int key]
        {
            get
            {
                return lineItems[key];
            }
            set
            {
                //add in empty spaces as needed
                while (lineItems.Count <= key)
                {
                    lineItems.Add("");
                }
                lineItems[key] = value;
            }
        }

        private string EscapeString(string text)
        {
            //escape quote marks
            text = text.Replace("\"", "\"\"");
            text = string.Format("\"{0}\"", text);

            //Optional: add other escapes as necessary
            return text;
        }

        public override string ToString()
        {
            //convert to array so that we don't mess with original values
            string[] items = lineItems.ToArray();

            //escape chars
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = EscapeString(items[i]);
            }

            //put into one string
            return string.Join(",", items);
        }
    }
}
