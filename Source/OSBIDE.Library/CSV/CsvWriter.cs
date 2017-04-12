using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.CSV
{
    public class CsvWriter
    {
        private List<CsvWriterLine> lines = new List<CsvWriterLine>();

        public CsvWriter()
        {
            //always start with one empty line
            lines.Add(new CsvWriterLine());
        }

        /// <summary>
        /// Adds a new cell to the current row
        /// </summary>
        /// <param name="content"></param>
        public void AddToCurrentLine(string content)
        {
            lines.Last().Add(content);
        }

        /// <summary>
        /// Adds a new cell to the current row
        /// </summary>
        /// <param name="content"></param>
        public void AddToCurrentLine(int content)
        {
            lines.Last().Add(content.ToString());
        }

        /// <summary>
        /// Adds a new cell to the current row
        /// </summary>
        /// <param name="content"></param>
        public void AddToCurrentLine(double content)
        {
            lines.Last().Add(content.ToString());
        }

        /// <summary>
        /// Will skip down to the next row in the CSV file.  
        /// This will cause all future default-writes (e.g. <see cref="AddToCurrentLine"/>)
        /// to write to this new line.
        /// </summary>
        public void CreateNewRow()
        {
            lines.Add(new CsvWriterLine());
        }

        /// <summary>
        /// Handy for skipping around in the CSV file.  Will throw an error if trying to access
        /// an invalid line.  Will automatically make space when setting to an invalid line.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CsvWriterLine this[int key]
        {
            get
            {
                return lines[0];
            }
            set
            {
                //add in empty lines as necessary
                while (key >= lines.Count)
                {
                    lines.Add(new CsvWriterLine());
                }
                lines[key] = value;
            }
        }

        /// <summary>
        /// Outputs to CSV string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string[] csvLines = new string[lines.Count];
            int counter = 0;
            foreach (CsvWriterLine line in lines)
            {
                csvLines[counter] = line.ToString();
                counter++;
            }
            return string.Join(Environment.NewLine, csvLines);
        }

        /// <summary>
        /// Converts CSV string into Stream for writing
        /// </summary>
        /// <returns></returns>
        public Stream ToStream()
        {
            MemoryStream ms = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(ms))
            {
                writer.Write(this.ToString());
            }
            return ms;
            
        }
    }
}
