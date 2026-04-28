using System;

namespace StadiumSystem.UI;

public static class ConsoleHelper
{
    public static void ShowSuccess(string message = "")
    {
        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine(message);
        }
        System.Threading.Thread.Sleep(1200);
    }

    public static int ShowInteractiveMenu(string title, string subtitle, string[] options)
    {
        int selectedIndex = 0;
        ConsoleKey key;
        Console.CursorVisible = false;

        Console.Clear();
        
        do
        {
            Console.SetCursorPosition(0, 0); 
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
            
            if (!string.IsNullOrEmpty(subtitle))
            {
                Console.WriteLine(subtitle);
                Console.WriteLine(new string('-', title.Length));
            }
            
            // Requerimiento explícito: Siempre mostrar esto en todos los menús
            Console.WriteLine("\n[ESC] para regresar o salir.\n");
            
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($" > {options[i]} ".PadRight(55)); 
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"   {options[i]} ".PadRight(55));
                }
            }

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex--;
                if (selectedIndex < 0) selectedIndex = options.Length - 1;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex++;
                if (selectedIndex >= options.Length) selectedIndex = 0;
            }
            else if (key == ConsoleKey.Escape)
            {
                Console.CursorVisible = true;
                return -1;
            }

        } while (key != ConsoleKey.Enter);

        Console.CursorVisible = true;
        return selectedIndex;
    }

    public static int ShowInteractiveMenu(
        string title,
        Func<string> subtitleProvider,
        string[] options,
        Func<string?>? flashProvider = null,
        int refreshMs = 150)
    {
        int selectedIndex = 0;
        Console.CursorVisible = false;
        Console.Clear();

        string lastSubtitle = "\0";
        string lastFlash = "\0";
        int lastSelected = -1;

        while (true)
        {
            string currentSubtitle = subtitleProvider() ?? "";
            string currentFlash = flashProvider?.Invoke() ?? "";

            if (currentSubtitle != lastSubtitle || currentFlash != lastFlash || selectedIndex != lastSelected)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(title.PadRight(Math.Max(title.Length, 60)));
                Console.WriteLine(new string('-', title.Length));

                if (!string.IsNullOrEmpty(currentSubtitle))
                {
                    Console.WriteLine(currentSubtitle.PadRight(Math.Max(title.Length, 60)));
                    Console.WriteLine(new string('-', title.Length));
                }

                Console.WriteLine("\n[ESC] para regresar o salir.".PadRight(60));

                if (!string.IsNullOrEmpty(currentFlash))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(currentFlash.PadRight(60));
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(new string(' ', 60));
                }
                Console.WriteLine();

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" > {options[i]} ".PadRight(55));
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"   {options[i]} ".PadRight(55));
                    }
                }

                lastSubtitle = currentSubtitle;
                lastFlash = currentFlash;
                lastSelected = selectedIndex;
            }

            DateTime deadline = DateTime.UtcNow.AddMilliseconds(refreshMs);
            while (DateTime.UtcNow < deadline)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.UpArrow)
                    {
                        selectedIndex--;
                        if (selectedIndex < 0) selectedIndex = options.Length - 1;
                    }
                    else if (key == ConsoleKey.DownArrow)
                    {
                        selectedIndex++;
                        if (selectedIndex >= options.Length) selectedIndex = 0;
                    }
                    else if (key == ConsoleKey.Escape)
                    {
                        Console.CursorVisible = true;
                        return -1;
                    }
                    else if (key == ConsoleKey.Enter)
                    {
                        Console.CursorVisible = true;
                        return selectedIndex;
                    }
                    break;
                }
                System.Threading.Thread.Sleep(15);
            }
        }
    }

    public static string? ReadInputWithEsc()
    {
        string input = "";
        ConsoleKeyInfo info;
        do
        {
            info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Escape) return null; 
            
            if (info.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input = input.Substring(0, input.Length - 1);
                Console.Write("\b \b");
            }
            else if (info.KeyChar != '\u0000' && info.Key != ConsoleKey.Enter && info.Key != ConsoleKey.Backspace)
            {
                input += info.KeyChar;
                Console.Write(info.KeyChar);
            }
        } while (info.Key != ConsoleKey.Enter);
        
        return input;
    }

    public static string? ReadPasswordWithEsc()
    {
        string password = "";
        ConsoleKeyInfo info;
        do
        {
            info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Escape) return null; 
            
            if (info.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
            else if (info.KeyChar != '\u0000' && info.Key != ConsoleKey.Enter && info.Key != ConsoleKey.Backspace)
            {
                Console.Write("*");
                password += info.KeyChar;
            }
        } while (info.Key != ConsoleKey.Enter);
        
        return password;
    }
}
