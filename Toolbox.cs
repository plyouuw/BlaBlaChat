static class Toolbox
{

    public static void DoItXTimes(int count, Action action)
    {
        for (int i = 0; i <= count; i++)
        {
            action();
        }
    }

    public class Menu
    {
        public enum Theme
        {
            None,
            RedArrow,
            GreenArrow,
            BlueArrow,
            YellowArrow,
            GrayArrow,
            WhiteArrow,
            RedBacklight,
            GreenBacklight,
            BlueBacklight,
            YellowBacklight,
            GrayBacklight,
            WhiteBacklight
        }
        private readonly Dictionary<int, string> SlownikOpcji;
        private string KomunikatPoczatkowy = string.Empty;
        private string KomunikatKoncowy = string.Empty;
        private ConsoleColor BacklightColor = ConsoleColor.Black;
        private ConsoleColor ArrowColor = ConsoleColor.Black;

        public void SetStyle(Theme styl)
        {
            if (styl == Theme.RedArrow)
                ArrowColor = ConsoleColor.Red;
            else if (styl == Theme.GreenArrow)
                ArrowColor = ConsoleColor.Green;
            else if (styl == Theme.BlueArrow)
                ArrowColor = ConsoleColor.Blue;
            else if (styl == Theme.YellowArrow)
                ArrowColor = ConsoleColor.Yellow;
            else if (styl == Theme.GrayArrow)
                ArrowColor = ConsoleColor.DarkGray;
            else if (styl == Theme.WhiteArrow)
                ArrowColor = ConsoleColor.White;

            else if (styl == Theme.RedBacklight)
                BacklightColor = ConsoleColor.Red;
            else if (styl == Theme.GreenBacklight)
                BacklightColor = ConsoleColor.Green;
            else if (styl == Theme.BlueBacklight)
                BacklightColor = ConsoleColor.Blue;
            else if (styl == Theme.YellowBacklight)
                BacklightColor = ConsoleColor.Yellow;
            else if (styl == Theme.GrayBacklight)
                BacklightColor = ConsoleColor.DarkGray;
            else if (styl == Theme.WhiteBacklight)
                BacklightColor = ConsoleColor.White;
        }
        public void SetKomunikatPoczatkowy(string komunukat_poczatkowy) => this.KomunikatPoczatkowy = komunukat_poczatkowy;
        public void SetKomunikatKoncowy(string komunukat_koncowy) => this.KomunikatKoncowy = komunukat_koncowy;
        public Menu(Theme styl, Dictionary<int, string> opcje_wyboru, string komunikat_poczatkowy = "", string komunikat_koncowy = "\nUżyj klawisza ENTER aby zatwierdzić wybór")
        {
            SlownikOpcji = opcje_wyboru;
            KomunikatPoczatkowy = komunikat_poczatkowy;
            KomunikatKoncowy = komunikat_koncowy;
            SetStyle(styl);
        }
        private void Print(int selected = 0, string optional_before_text = "")
        {
            Console.Clear();
            if (optional_before_text != "") Console.WriteLine(optional_before_text);
            if (KomunikatPoczatkowy != "") Console.WriteLine(KomunikatPoczatkowy + "\n");
            foreach (var item in SlownikOpcji)
            {
                if (BacklightColor != ConsoleColor.Black)
                {
                    if (selected == SlownikOpcji.ToList().IndexOf(item))
                    {
                        Console.BackgroundColor = BacklightColor;
                        if (BacklightColor == ConsoleColor.White || BacklightColor == ConsoleColor.Yellow) Console.ForegroundColor = ConsoleColor.Black;
                        else Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (ArrowColor != ConsoleColor.Black)
                {
                    if (selected == SlownikOpcji.ToList().IndexOf(item))
                    {
                        Console.ForegroundColor = ArrowColor;
                        Console.Write("> ");
                        Console.ResetColor();
                    }
                    else Console.Write("  ");
                }
                else
                {
                    if (selected == SlownikOpcji.ToList().IndexOf(item))
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                }
                Console.WriteLine(item.Key.ToString() + ". " + item.Value + " ");
                Console.ResetColor();
            }
            if (KomunikatKoncowy != "") Console.WriteLine(KomunikatKoncowy);
        }
        public int ReadChoice(string optionaly_before_text = "")
        {
            int selected = 0;
            bool done = true;
            ConsoleKey key;

            while (done)
            {
                Print(selected, optionaly_before_text);
                key = Console.ReadKey().Key;
                if (key == ConsoleKey.UpArrow)
                {
                    if (selected == 0) selected = SlownikOpcji.Count - 1;
                    else selected--;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    if (selected == SlownikOpcji.Count - 1) selected = 0;
                    else selected++;
                }
                else if (key == ConsoleKey.Enter) done = false;
                else if (ZeroToNine())
                {
                    if ((int)key <= 57 && (int)key >= 48 && IndexById((int)key - 48) != -1) selected = IndexById((int)key - 48);
                    else if ((int)key <= 105 && (int)key >= 96 && IndexById((int)key - 96) != -1) selected = IndexById((int)key - 96);
                }
            }
            if (selected < 0 || selected > SlownikOpcji.Count) return 0;
            return SlownikOpcji.ElementAt(selected).Key;
        }

        private bool ZeroToNine()
        {
            foreach (var pair in SlownikOpcji)
                if (pair.Key > 9)
                    return false;
            return true;
        }
        private int IndexById(int id)
        {
            int index = -1;
            foreach (var item in SlownikOpcji)
            {
                if (item.Key == id) index = SlownikOpcji.ToList().IndexOf(item);
            }
            return index;
        }
    }

    public static void Print(object? value, ConsoleColor kolor_tekstu = ConsoleColor.Gray, ConsoleColor kolor_tla = ConsoleColor.Black)
    {
        Console.ForegroundColor = kolor_tekstu;
        Console.BackgroundColor = kolor_tla;
        Console.Write(value ?? "null");
        Console.ResetColor();
    }

    public static void Println(object? value, ConsoleColor kolor_tekstu = ConsoleColor.Gray, ConsoleColor kolor_tla = ConsoleColor.Black)
    {
        Console.ForegroundColor = kolor_tekstu;
        Console.BackgroundColor = kolor_tla;
        Console.WriteLine(value ?? "null");
        Console.ResetColor();
    }
    public static void Println()
    {
        Console.WriteLine();
    }
    public static void PrintError(object text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(text);
        Console.ResetColor();
    }
    public static bool StringToInteger(string input, ref int output)
    {
        try
        {
            output = int.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static string RepeatString(string text, int count_of_times)
    {
        return string.Concat(Enumerable.Repeat(text, count_of_times));
    }
    public static void Repeat(int count, Action action)
    {
        for (int i = 0; i <= count; i++)
        {
            action();
        }
    }
    public static void Debug(string name, object? value) => Print("[DEBUG] " + name + " => " + value + "\n");
    public static void Debug(object value) => Print("[DEBUG] " + value + "\n");
    public static void Czekaj(bool czy_komunikat = true)
    {
        if (czy_komunikat) Print("\n\nAby kontynuować, naciśnij dowolny klawisz\n");
        Console.ReadKey(false);
    }
    public static string[]? WczytajPlik(string nazwa_pliku)
    {
        string[] wczytany_plik;
        try
        {
            wczytany_plik = File.ReadAllLines(nazwa_pliku);
        }
        catch (Exception e)
        {
            PrintError("Wystąpił błąd podczas odczytu pliku: \n");
            PrintError(e.Message);
            return null;
        }
        return wczytany_plik;
    }
    public static bool ZapiszPlik(string nazwa_pliku, string dane)
    {
        try
        {
            File.WriteAllText(nazwa_pliku, dane);
        }
        catch (Exception e)
        {
            PrintError("Wystąpił błąd podczas zapisu pliku:\n");
            PrintError(e.Message);
            return false;
        }
        return true;
    }
    public static string Read() => Console.ReadLine() ?? "";
    public static string Read(string before)
    {
        Print(before);
        return Console.ReadLine() ?? "";
    }

    public static int ReadInt(bool czy_powtarzac_probe, string komunikat = "", string komunikat_bledu = "", int wartosc_minimalna = int.MinValue, int wartosc_maksymalna = int.MaxValue)
    {
        int wynik;
        Print(komunikat);
        while (!(int.TryParse(Read(), out wynik) && wynik >= wartosc_minimalna && wynik <= wartosc_maksymalna))
        {
            if (komunikat_bledu != "") PrintError(komunikat_bledu);
            if (!czy_powtarzac_probe)
            {
                wynik = int.MinValue;
                break;
            }
            else Print(komunikat);
        }

        return wynik;
    }
}

