using System.Net;
using System.Net.Sockets;
using System.Text;
using static Toolbox;

namespace chat_tcp
{
    internal class Server
    {

        internal class Client
        {
            public TcpClient TCP;
            private List<Client> lista_clientow;
            public string Name { get; set; } = "";
            public string ID { get; }
            public Client(ref List<Client> lista_clientow, TcpClient TCP)
            {
                this.TCP = TCP;
                this.lista_clientow = lista_clientow;
                if (TCP.Client.RemoteEndPoint == null) ID = "null";
                else ID = (TCP.Client.RemoteEndPoint.ToString()??":null").Split(':')[1];
            }

            private void SendToAllClients(string data)
            {
                foreach(Client c in lista_clientow)
                {
                    if (!c.TCP.Connected || c.TCP.Client.RemoteEndPoint == TCP.Client.RemoteEndPoint) continue;

                    try
                    {
                        c.TCP.GetStream().Write(Encoding.UTF8.GetBytes(data));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            public void Polaczenie()
            {
                while (IsConnected())
                {
                    if (AvailableData())
                    {
                        string data = GetData();
                        if(Name == "")
                        {
                            if(data.Length > 0)
                            {
                                Name = data;
                                SendData(ID);
                            }
                        } 
                        else
                        {
                            if (data.Length > 0)
                            {
                                SendToAllClients($"{Name}#{ID}:{data}");
                            }
                        }
                    }
                }

                if (IsConnected()) TCP.Close();
                lista_clientow.Remove(this);
            }

            public bool IsConnected()
            {
                if (TCP == null) return false;
                if (TCP.Client == null) return false;
                try
                {
                    if (!TCP.Client.Connected)
                        return false;
                    // Detect if client disconnected
                    if (TCP.Client != null && TCP.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (TCP.Client.Receive(buff, SocketFlags.Peek) == 0)
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
                return TCP.Available > 0;
            }


            // Metody

            public void SendData(string data)
            {
                if (!TCP.Connected)
                {
                    lista_clientow.Remove(this);
                    return;
                }
                try
                {
                    TCP.GetStream().Write(Encoding.UTF8.GetBytes(data));
                }
                catch
                {
                    PrintError("Wyst¹pi³ problem z po³¹czeniem z serwerem! (SendText: stream.Write exception)");
                }
            }

            public string GetData()
            {
                if (!TCP.Connected)
                {
                    lista_clientow.Remove(this);
                    return "";
                }

                if (!AvailableData()) return "";

                byte[] buffer = new byte[4096];
                int received;
                if (TCP.GetStream().DataAvailable)
                {
                    try
                    {
                        received = TCP.GetStream().Read(buffer, 0, buffer.Length);
                    }
                    catch (Exception e)
                    {
                        PrintError($"Wyst¹pi³ problem z po³¹czeniem z serwerem! (GetData: reading data - {e.Message})");
                        return "";
                    }
                    string encoded = Encoding.UTF8.GetString(buffer, 0, received);
                    string handled = IncomingHandler(encoded);
                    if (handled == encoded) return encoded;
                    else
                    {
                        SendData("Server#00001:" + handled);
                        return "";
                    }


                }
                else return "";
            }

            public void Close()
            {
                TCP.Close();
            }

            private string IncomingHandler(string data)
            {
                if (data[0] != '/')
                {
                    return data;
                }

                string response = "";

                switch (data[1..])
                {
                    case "quit":
                    case "bye":
                        if (IsConnected()) TCP.Close();
                        response = "bye";
                        break;
                    case "date":
                        response = DateTime.Now.ToString();
                        break;
                    case "clients_list":
                        foreach (Client client in lista_clientow)
                        {
                            if (client.IsConnected()) response += $"{client.Name}={client.TCP.Client.RemoteEndPoint};";
                        }
                        break;
                    case "help":
                    case "?":
                        response = "quit date clients_list";
                        break;
                    default:
                        response = "U¿yj /help lub /?, aby uzyskaæ listê dostêpnych komend.";
                        break;
                }


                return response;
            }
        }

        private readonly TcpListener listener;
        private string IP { get; }
        private int Port { get; }
        private List<Client> clients;
        public Server(string IpAddress, int port)

        {
            IP = IpAddress;
            Port = port;
            listener = new(IPAddress.Parse(IP), Port);
            clients = new();
        }

        public void Start()
        {
            Console.Clear();
            Console.TreatControlCAsInput = true;
            try
            {
                listener.Start();
            }
            catch
            {
                PrintError("\nWyst¹pi³ problem z uruchomieniem serwera!\n");
                return;
            }

            Thread watek;
            Client client;
            ConsoleKeyInfo cki;
            int temp_count = 0;
            Println($"Liczba po³¹czonych klientów: {temp_count}   IP: {IP}   Port: {Port}");
            while (true)
            {
                if (temp_count != clients.Count)
                {
                    temp_count = clients.Count;
                    int temptop = Console.CursorTop, templeft = Console.CursorLeft;
                    Console.CursorTop = 0;
                    Console.CursorLeft = 0;
                    Print(new string(' ', Console.BufferWidth));
                    Console.CursorLeft = 0;
                    Println($"Liczba po³¹czonych klientów: {temp_count}   IP: {IP}   Port: {Port}");
                    Console.CursorTop = temptop;
                    Console.CursorLeft = templeft;
                }
                if (listener.Pending()) 
                {
                    client = new(ref clients, listener.AcceptTcpClient());
                    watek = new(client.Polaczenie);
                    watek.Start();
                    lock (clients) clients.Add(client);
                }
                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.C && cki.Modifiers == ConsoleModifiers.Control)
                    {
                        lock (clients)
                        {
                            foreach (Client c in clients)
                            {
                                if (c.IsConnected())
                                {
                                    c.Close();
                                }
                            }
                            clients.Clear();
                        }
                        break;
                    }
                }
            }
            Console.TreatControlCAsInput = false;
            Print("\nZakoñczono pracê serwera.\n");
        }
    }
}