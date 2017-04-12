using OSBIDE.Analytics.Terminal.ViewModels;
using OSBIDE.Library.CSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Views
{
    public class ActionRequestLogView
    {

        private enum MenuOption { LoadLogsByDate = 1, LoadRawLogsFromDb, LoadRawLogsFromCache, DetailsViewCounts, AggregateByWeek, GenerateWeeklyStatistics, CountProfileViews, Exit }

        public void Run()
        {
            int userChoice = 0;
            ActionRequestLogViewModel vm = new ActionRequestLogViewModel();
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine((int)MenuOption.LoadLogsByDate + ". Reload access logs BY DATE from DB (costly)");
                Console.WriteLine((int)MenuOption.LoadRawLogsFromDb + ". Reload RAW access logs from DB (costly)");
                Console.WriteLine((int)MenuOption.LoadRawLogsFromCache + ". Load RAW access logs from cache");
                Console.WriteLine((int)MenuOption.DetailsViewCounts + ". Get details view statistics");
                Console.WriteLine((int)MenuOption.AggregateByWeek + ". Aggregate daily activity by week");
                Console.WriteLine((int)MenuOption.GenerateWeeklyStatistics + ". Generate summary weekly statistics by controller and action");
                Console.WriteLine((int)MenuOption.CountProfileViews + ". Count profile views");
                
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
                        case MenuOption.LoadLogsByDate:
                            vm.LoadLogsByDate();
                            Console.WriteLine("access logs parsed...");
                            break;
                        case MenuOption.LoadRawLogsFromDb:
                            vm.LoadRawLogsFromDb();
                            Console.WriteLine("raw logs loaded...");
                            break;
                        case MenuOption.LoadRawLogsFromCache:
                            vm.LoadRawLogsFromCache();
                            break;
                        case MenuOption.DetailsViewCounts:
                            var viewCounts = vm.GenerateDetailsViewStatistics();

                            foreach (var kvp in viewCounts)
                            {
                                //unique students
                                Dictionary<int, int> students = new Dictionary<int, int>();
                                foreach(var actionLog in kvp.Value)
                                {
                                    if(students.ContainsKey(actionLog.CreatorId) == false)
                                    {
                                        students.Add(actionLog.CreatorId, 1);
                                    }
                                    students[actionLog.CreatorId] += 1;
                                }

                                
                                CsvWriter writer = new CsvWriter();
                                writer.AddToCurrentLine(kvp.Key);
                                writer.CreateNewRow();
                                foreach(var student in students)
                                {
                                    writer.AddToCurrentLine(student.Value);
                                    writer.CreateNewRow();
                                }
                                using (TextWriter tw = File.CreateText(kvp.Key + ".csv"))
                                {
                                    tw.Write(writer.ToString());
                                }
                                Console.WriteLine("{0}: {1} (num students: {2})", kvp.Key, kvp.Value.Count, students.Keys.Count);
                                
                            }
                            break;
                        case MenuOption.AggregateByWeek:
                            List<List<string>> spreadsheet = vm.AggregateLogsByWeek();
                            Console.Write("Enter destination file: ");
                            string fileName = Console.ReadLine();
                            vm.WriteToCsv(spreadsheet, fileName);
                            Console.WriteLine("daily activity aggregated...");
                            break;
                        case MenuOption.GenerateWeeklyStatistics:

                            //aggreate weekly statistics
                            vm.AggregateLogsByWeek();

                            //generate one CSV file for each controller / action pairing
                            foreach(string controllerName in vm.ControllerActions.Keys)
                            {
                                foreach(string actionName in vm.ControllerActions[controllerName].Keys)
                                {
                                    List<List<string>> matrix = vm.FilterActivity(controllerName, actionName);
                                    string name = string.Format("weekly_{0}_{1}.csv", controllerName, actionName);
                                    vm.WriteToCsv(matrix, name);
                                }
                            }

                            break;
                        case MenuOption.CountProfileViews:
                            var result = vm.CountProfileVisits();
                            foreach(var kvp in result)
                            {
                                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
                            }
                            break;
                        case MenuOption.Exit:
                            Console.WriteLine("Returning to main menu.");
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("");
            }
        }
    }
}
