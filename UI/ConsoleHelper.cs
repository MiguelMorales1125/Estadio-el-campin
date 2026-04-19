using System;

namespace StadiumSystem.UI;

/// <summary>
/// GRASP: Pure Fabrication.
/// Se extrajo toda la lógica repetitiva y pesada de la consola a esta clase helper
/// para mantener Program.cs completamente limpio y enfocado a flujo de negocio.
/// </summary>
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

    /// <summary>
    /// Dibuja un menú dinámico y maneja la lógica de las flechas nativas y cancelación con Escape.
    /// </summary>
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

    public static string? ReadInputWithEsc()
    {
        string input = "";
        ConsoleKeyInfo info;
        do
        {
            info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Escape) return null; // Abort
            
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
            if (info.Key == ConsoleKey.Escape) return null; // Abort
            
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
