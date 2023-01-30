using chat_tcp;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static Toolbox;

namespace BlaBlaChat
{
    internal class Executor
    {
        const string IP = "127.0.0.1";
        const int PORT = 8888;
        
        static void PrintHeader(string additional_data = "")
        {
            Print("Witaj w ");
            Print("Bla", ConsoleColor.Green);
            Print("Bla", ConsoleColor.Blue);
            Print("Chat", ConsoleColor.Yellow);
            if (additional_data != "")
            {
                Println(additional_data);
            }
            else Println();
        }

        static string EnterName(int min_chars, int max_chars)
        {
            string name = "";
            string temp_name = "";
            while (true)
            {
                if (temp_name != name)
                {
                    temp_name = name;
                    Console.CursorLeft = 0;
                    Print(new string(' ', Console.BufferWidth));
                    Console.CursorLeft = 0;
                    Print("> ", ConsoleColor.White);
                    Print(name);
                }
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo temp_cki = Console.ReadKey(true);
                    if (temp_cki.Key == ConsoleKey.Enter)
                    {
                        if (name.Length >= min_chars)
                        {
                            Println();
                            break;
                        }
                    }
                    else if (temp_cki.Key == ConsoleKey.Backspace)
                    {
                        if (name.Length > 0)
                        {
                            name = name.Remove(name.Length - 1);
                        }

                    }
                    else if (temp_cki.Key >= ConsoleKey.A &&
                             temp_cki.Key <= ConsoleKey.Z ||
                             temp_cki.Key >= ConsoleKey.D0 &&
                             temp_cki.Key <= ConsoleKey.D9 ||
                             temp_cki.Key >= ConsoleKey.NumPad0 &&
                             temp_cki.Key <= ConsoleKey.Divide ||
                             temp_cki.Key >= ConsoleKey.Oem1 &&
                             temp_cki.Key <= ConsoleKey.Oem102 ||
                             temp_cki.Key == ConsoleKey.Spacebar &&
                             temp_cki.Modifiers != ConsoleModifiers.Control
                            )
                    {
                        if (name.Length == 0 && temp_cki.Key == ConsoleKey.Spacebar) continue;
                        if (name.Length >= max_chars) continue;
                        name += temp_cki.KeyChar;
                    }
                }
            }

            return name;
        }

        static void PrintMessage(string read, string prefix, string id, string message)
        {
            Console.CursorLeft = 0;
            Print($"[");
            Print(prefix, ConsoleColor.White);
            if(id != "") Print("#" + id, ConsoleColor.DarkGray);
            Println($"] {message}");
            Console.CursorLeft = 0;
            Print($"> {read}");
        }

        static string ShortText(string text)
        {
            if (text.Length > Console.BufferWidth - 2)
            {
                return text[(text.Length - Console.BufferWidth + 2)..];
            }
            else return text;
        }

        static void Main()
        {
            char c = Console.ReadKey(true).KeyChar;
            if(c == 's')
            {
                Server s = new(IP, PORT);
                s.Start();
                return;
            }
            PrintHeader();

            Println("Inicjalizuję połączenie z serwerem...", ConsoleColor.White);
            Client client = new(IP, PORT);
            if (!client.Connect()) return;
            Println("Nawiązano połączenie!", ConsoleColor.White);

            Print("Podaj swój nick, zanim rozpoczniesz rozmowę: (min. 3 znaki, max. 12 znaków)\n> ", ConsoleColor.White);
            string name = EnterName(3, 12);

            client.Name = name;
            client.SendData(name);
            while (!client.AvailableData()) ;
            client.ID = client.GetData();

            Console.Clear();
            PrintHeader($"\tNick: {name}  Adres serwera: {IP}:{PORT}");

            Print("> ");
            string input = "", temp_input = "";
            Thread reading = new(() =>
            {
                while (client.IsConnected())
                {
                    if (temp_input != input)
                    {
                        temp_input = input;
                        Console.CursorLeft = 0;
                        Print(new string(' ', Console.BufferWidth));
                        Console.CursorLeft = 0;
                        Print("> " + ShortText(temp_input));
                    }
                    if (client.AvailableData())
                    {
                        string data = client.GetData();
                        if (data.Length > 0)
                        {
                            if (data.Split(':').Length > 1)
                            {
                                if (data.Split(':')[0].Contains('#'))
                                {
                                    PrintMessage(input, data.Split(':')[0].Split('#')[0], data.Split(':')[0].Split('#')[1], data[(data.IndexOf(':') + 1)..]);
                                }
                                else PrintMessage(input, data.Split(':')[0], "null", data[(data.IndexOf(':') + 1)..]);
                            }
                        }

                    }
                }
            });
            reading.Start();
            while (client.IsConnected())
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo temp_cki = Console.ReadKey(true);
                    if (temp_cki.Key == ConsoleKey.Enter)
                    {
                        if (input.Trim().Length > 0)
                        {
                            client.SendData(input);
                            if (input.ElementAt(0) != '/') PrintMessage("", client.Name, client.ID, input);
                            //else PrintMessage("", "Command", read);
                            input = "";
                        }
                    }
                    else if (temp_cki.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length > 0)
                        {
                            input = input.Remove(input.Length - 1);
                        }

                    }
                    else if (temp_cki.Key >= ConsoleKey.A &&
                             temp_cki.Key <= ConsoleKey.Z ||
                             temp_cki.Key >= ConsoleKey.D0 &&
                             temp_cki.Key <= ConsoleKey.D9 ||
                             temp_cki.Key >= ConsoleKey.NumPad0 &&
                             temp_cki.Key <= ConsoleKey.Divide ||
                             temp_cki.Key >= ConsoleKey.Oem1 &&
                             temp_cki.Key <= ConsoleKey.Oem102 ||
                             //temp_cki.Key == ConsoleKey.Tab ||
                             temp_cki.Key == ConsoleKey.Spacebar &&
                             temp_cki.Modifiers != ConsoleModifiers.Control
                            )
                    {
                        if (input.Length == 0 && temp_cki.Key == ConsoleKey.Spacebar) continue;
                        input += temp_cki.KeyChar;
                    }
                }
            }
        }
    }

    internal class Client
    {
        private (string IP, int Port) Address;
        private TcpClient tcp;
        public string Name { get; set; } = "Client";
        public string ID { get; set; } = "null";
        public Client(string ip, int port)
        {
            try
            {
                IPAddress.Parse(ip);
                Address.IP = ip;
            }
            catch
            {
                PrintError("Wystąpił błąd przy parsowaniu adresu IP");
                Address.IP = "";
            }
            finally
            {
                Address.Port = port;
                tcp = new TcpClient();
            }

        }

        // Zarządzanie połączeniem

        public bool Connect()
        {
            if (tcp.Connected)
            {
                PrintError("Połączenie zostało już ustanowione.");
                return false;
            }
            try
            {
                tcp = new(Address.IP, Address.Port);
            }
            catch
            {
                PrintError($"Wystąpił problem z ustanowieniem połączenia ze zdalnym serwerem! {Address.IP}:{Address.Port}\n");
                return false;
            }

            return true;
        }

        public bool IsConnected()
        {
            try
            {
                if (!tcp.Client.Connected)
                    return false;
                // Detect if client disconnected
                if (tcp.Client != null && tcp.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (tcp.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool AvailableData()
        {
            return tcp.Available > 0;
        }


        // Metody

        public void SendData(string data)
        {
            if (!tcp.Connected)
            {
                PrintError("Wystąpił problem z połączeniem z serwerem! (SendText: tcp is not connected)");
                return;
            }
            try
            {
                tcp.GetStream().Write(Encoding.UTF8.GetBytes(data));
            }
            catch
            {
                PrintError("Wystąpił problem z połączeniem z serwerem! (SendText: stream.Write exception)");
            }
        }

        public string GetData()
        {
            if (!tcp.Connected)
            {
                PrintError("Wystąpił problem z połączeniem z serwerem! (SendText: tcp is not connected)");
                return "";
            }

            if (!AvailableData()) return "";

            byte[] buffer = new byte[4096];
            int received;
            if (tcp.GetStream().DataAvailable)
            {
                try
                {
                    received = tcp.GetStream().Read(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    PrintError($"Wystąpił problem z połączeniem z serwerem! (GetData: reading data - {e.Message})");
                    return "";
                }
                return Encoding.UTF8.GetString(buffer, 0, received);


            }
            else
            {
                PrintError("Wystąpił problem z połączeniem z serwerem! (GetData: data is not available)");
                return "";
            }
        }
    }
}