using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSBIDE.Analytics.Terminal.Views;

namespace OSBIDE.Analytics.Terminal
{
    class Program
    {
        private enum MenuOption { CommentMetrics = 1, TimelineAnalysis, ActionRequestLog, ContentCoding,  Exit }
        static void Main(string[] args)
        {

            int userChoice = 0;
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine("*** OSBIDE Analytics ***");
                Console.WriteLine((int)MenuOption.CommentMetrics + ". Run comment metrics");
                Console.WriteLine((int)MenuOption.TimelineAnalysis + ". Run timeline analysis");
                Console.WriteLine((int)MenuOption.ActionRequestLog + ". Process action request logs");
                Console.WriteLine((int)MenuOption.ContentCoding + ". Process coded feed items");
                Console.WriteLine((int)MenuOption.Exit + ". Exit");
                Console.Write(">> ");

                string rawInput = Console.ReadLine();
                if (Int32.TryParse(rawInput, out userChoice) == false)
                {
                    Console.WriteLine("Invalid input.");
                }
                else
                {
                    MenuOption selection = (MenuOption)userChoice;
                    switch (selection)
                    {
                        case MenuOption.CommentMetrics:
                            CommentMetricsView view = new CommentMetricsView();
                            view.Run();
                            break;
                        case MenuOption.TimelineAnalysis:
                            TimelineAnalysisView tav = new TimelineAnalysisView();
                            tav.Run();
                            break;
                        case MenuOption.ActionRequestLog:
                            ActionRequestLogView arlv = new ActionRequestLogView();
                            arlv.Run();
                            break;
                        case MenuOption.ContentCoding:
                            ContentCodingView ccv = new ContentCodingView();
                            ccv.Run();
                            break;
                        case MenuOption.Exit:
                            Console.WriteLine("Exiting program.");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
