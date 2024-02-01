using System;

namespace MyConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to my console application!");
            Console.WriteLine("Please enter your name:");

            string name = Console.ReadLine();

            Console.WriteLine($"Hello, {name}! This is your first C# console application.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
using System;

public class Token
{
    private string value;

    public Token(string value)
    {
        this.value = value;
    }

    public string GetValue()
    {
        return value;
    }

    public void SetValue(string newValue)
    {
        value = newValue;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Token myToken = new Token("FTL::Token");

        Console.WriteLine("Token value: " + myToken.GetValue());

        myToken.SetValue("newTokenValue");

        Console.WriteLine("New token value: " + myToken.GetValue());
    }
}
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class Block
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public string PreviousHash { get; set; }
    public string Hash { get; set; }
    public string Data { get; set; }
    public int Nonce { get; set; }

    public Block(DateTime timestamp, string previousHash, string data)
    {
        Index = 0;
        Timestamp = timestamp;
        PreviousHash = previousHash;
        Data = data;
        Hash = CalculateHash();
        Nonce = 0;
    }

    public string CalculateHash()
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            string rawData = $"{Index}-{Timestamp}-{PreviousHash ?? ""}-{Data}-{Nonce}";
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }
    }

    public void MineBlock(int difficulty)
    {
        string leadingZeros = new string('0', difficulty);
        while (Hash.Substring(0, difficulty) != leadingZeros)
        {
            Nonce++;
            Hash = CalculateHash();
        }
        Console.WriteLine("Block mined: " + Hash);
    }
}

public class Blockchain
{
    public List<Block> Chain { get; set; }
    public int Difficulty { get; set; }

    public Blockchain(int difficulty)
    {
        Difficulty = difficulty;
        Chain = new List<Block> { CreateGenesisBlock() };
    }

    public Block CreateGenesisBlock()
    {
        return new Block(DateTime.Now, null, "Genesis Block");
    }

    public Block GetLatestBlock()
    {
        return Chain[Chain.Count - 1];
    }

    public void AddBlock(Block newBlock)
    {
        newBlock.PreviousHash = GetLatestBlock().Hash;
        newBlock.MineBlock(Difficulty);
        Chain.Add(newBlock);
    }

    public bool IsChainValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            Block currentBlock = Chain[i];
            Block previousBlock = Chain[i - 1];

            if (currentBlock.Hash != currentBlock.CalculateHash())
            {
                return false;
            }

            if (currentBlock.PreviousHash != previousBlock.Hash)
            {
                return false;
            }
        }
        return true;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Blockchain blockchain = new Blockchain(4);

        Console.WriteLine("Mining block 1...");
        blockchain.AddBlock(new Block(DateTime.Now, null, "First block data"));

        Console.WriteLine("Mining block 2...");
        blockchain.AddBlock(new Block(DateTime.Now, null, "Second block data"));

        Console.WriteLine("Mining block 3...");
        blockchain.AddBlock(new Block(DateTime.Now, null, "Third block data"));

        Console.WriteLine("Is blockchain valid? " + blockchain.IsChainValid());

        // Attempt to tamper with the blockchain
        blockchain.Chain[1].Data = "Tampered data";
        Console.WriteLine("Is blockchain valid after tampering? " + blockchain.IsChainValid());
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Server
            if (args.Length > 0 && args[0] == "server")
            {
                await StartServerAsync();
            }
            // Client
            else if (args.Length > 0 && args[0] == "client")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: client <server IP> <file path>");
                    return;
                }
                string serverIp = args[1];
                string filePath = args[2];
                await StartClientAsync(serverIp, filePath);
            }
            else
            {
                Console.WriteLine("Usage: <server|client>");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task StartServerAsync()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 12345);
        listener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        using (TcpClient client = await listener.AcceptTcpClientAsync())
        using (NetworkStream stream = client.GetStream())
        using (var fileStream = File.OpenRead("server_file.txt"))
        {
            await fileStream.CopyToAsync(stream);
            Console.WriteLine("File sent to client.");
        }
    }

    static async Task StartClientAsync(string serverIp, string filePath)
    {
        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync(IPAddress.Parse(serverIp), 12345);
            Console.WriteLine("Connected to server.");

            using (NetworkStream stream = client.GetStream())
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
                Console.WriteLine($"File received and saved as {filePath}.");
            }
        }
    }
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        StartServer();
        //StartClient();
    }

    static void StartServer()
    {
        // Establish the local endpoint for the socket (server)
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // Bind the socket to the local endpoint and listen for incoming connections
            listener.Bind(localEndPoint);
            listener.Listen(10);

            Console.WriteLine("Waiting for a connection...");

            // Accept incoming connection
            Socket handler = listener.Accept();

            // Data buffer
            byte[] bytes = new byte[1024];
            int bytesReceived = handler.Receive(bytes);
            string dataReceived = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
            Console.WriteLine($"Received from client: {dataReceived}");

            // Send response to client
            string response = "Server received your message.";
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            handler.Send(responseBytes);

            // Close the connection
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    static void StartClient()
    {
        // Connect to a remote device
        try
        {
            // Establish the remote endpoint for the socket
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Connect the socket to the remote endpoint
                sender.Connect(remoteEndPoint);
                Console.WriteLine($"Socket connected to {sender.RemoteEndPoint}");

                // Data to send to the server
                string message = "Hello from client!";
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket
                int bytesSent = sender.Send(messageBytes);
                Console.WriteLine($"Sent {bytesSent} bytes to server");

                // Receive the response from the server
                byte[] responseBytes = new byte[1024];
                int bytesReceived = sender.Receive(responseBytes);
                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesReceived);
                Console.WriteLine($"Response from server: {response}");

                // Close the socket
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
