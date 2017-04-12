using OSBIDE.Analytics.Terminal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Views
{
    public class ContentCodingView
    {
        private enum MenuOption { LoadOsbideLogs = 1, GetCategoryCounts, Exit }
        public void Run()
        {
            int userChoice = 0;
            ContentCodingViewModel vm = new ContentCodingViewModel();
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine((int)MenuOption.LoadOsbideLogs + ". Load coded OSBIDE feed items");
                Console.WriteLine((int)MenuOption.GetCategoryCounts + ". Get category counts");
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
                        case MenuOption.LoadOsbideLogs:
                            vm.LoadCodedOsbideItems();
                            Console.WriteLine("OSBIDE logs loaded...");
                            break;
                        case MenuOption.GetCategoryCounts:
                            var counts = vm.CategorizeItems();
                            foreach(var mainCategories in counts)
                            {
                                foreach(var subCategories in mainCategories.Value)
                                {
                                    Console.WriteLine("{0} - {1}: {2}", mainCategories.Key, subCategories.Key, subCategories.Value);
                                }
                                
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
