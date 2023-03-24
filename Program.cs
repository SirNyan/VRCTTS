using System;
using System.Threading;

namespace VRCTTS
{
    class Program
    {
        static Menu_Azure azureMenu;
        static Menu_Settings settingsMenu;
        static Writer writer;

        static int previousIndex;
        static int currentIndex;
        static bool flag;

        static string[] mainMenuItems = { "Azure STTTS         ", "IVONA - COMING SOON ", "Options             ", "Exit                "};

        static void Main(string[] args)
        {
            //Test test = new Test();

            //test.testOSC();

            settingsMenu = new Menu_Settings();
            azureMenu = new Menu_Azure();
            writer = new Writer();

            Console.CursorVisible = false;

            previousIndex = 0;
            currentIndex = 0;
            flag = false;

            ConsoleKeyInfo keyPressed;

            MainMenu();

            do {
                keyPressed = Console.ReadKey(true);

                switch (keyPressed.Key)
                {
                    case ConsoleKey.Enter:
                        GoToMenu();
                        break;
                    case ConsoleKey.Backspace:
                        flag = true;
                        break;
                    case ConsoleKey.UpArrow:
                        if (currentIndex != 0) { previousIndex = currentIndex; currentIndex--; UpdateMenu(); }
                        break;
                    case ConsoleKey.DownArrow:
                        if (currentIndex != 3) { previousIndex = currentIndex; currentIndex++; UpdateMenu(); }
                        break;
                    case ConsoleKey.W:
                        if (currentIndex != 0) { previousIndex = currentIndex; currentIndex--; UpdateMenu(); }
                        break;
                    case ConsoleKey.S:
                        if (currentIndex != 3) { previousIndex = currentIndex; currentIndex++; UpdateMenu(); }
                        break;
                    default:
                        break;
                }
            } while (!flag);
        }

        static private void MainMenu() {
            Console.Clear();
            writer.writeAt(5, 0, "VRCHAT Speech-To-Text-To-Speech by KT");
            for (int i = 0; i < mainMenuItems.Length; i++) {
                if (i == currentIndex)
                {
                    writer.writeAt(5, i + 2, mainMenuItems[i], ConsoleColor.Black, ConsoleColor.White);
                }
                else 
                {
                    writer.writeAt(5, i + 2, mainMenuItems[i], ConsoleColor.White, ConsoleColor.Black);
                }
            }
        }

        static private void UpdateMenu() {
            writer.ClearLine(previousIndex+2);
            writer.ClearLine(currentIndex+2);
            writer.writeAt(5, currentIndex+2, mainMenuItems[currentIndex], ConsoleColor.Black, ConsoleColor.White);
            writer.writeAt(5, previousIndex+2, mainMenuItems[previousIndex], ConsoleColor.White, ConsoleColor.Black);
        }

        static private void GoToMenu() {
            switch (currentIndex)
            {
                case 0:
                    azureMenu.LoadMenu();
                    settingsMenu.SaveSettings();
                    MainMenu();
                    break;
                case 1:

                    break;
                case 2:
                    settingsMenu.LoadMenu();
                    azureMenu.Update();
                    MainMenu();
                    break;
                case 3:
                    flag = true;
                    break;
                default:
                    break;
            }
        }
    }
}
