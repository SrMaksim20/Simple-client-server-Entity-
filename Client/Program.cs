using System.IO;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

internal class Client
{
    public static async Task Main(string[] args)
    {

        using TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("127.0.0.1", 8888);
        var stream = tcpClient.GetStream();

        List<byte> response = new List<byte>();

        int bytesRead = 10;
        string text;
        byte[] data;
        string log;
        string pass;
        string name;
        string age;
        while (true)
        {
            Console.WriteLine("Введите сообщение");
            text = Console.ReadLine();
            Data_output(text, stream);

            switch (text)
            {
                case "/Sing_in":
                    //Data_input( response, bytesRead, stream);
                    Console.WriteLine("Введите логин:");
                    log = Console.ReadLine();
                    Data_output(log, stream);

                    //Data_input( response, bytesRead, stream);
                    Console.WriteLine("Введите пароль:");
                    pass = Console.ReadLine();
                    Data_output(pass, stream);

                    Data_input(response, bytesRead, stream);
                    break;
                case "/Sing_up":
                    Console.WriteLine("Введите логин:");
                    log = Console.ReadLine();
                    Data_output(log, stream);

                    Console.WriteLine("Введите пароль:");
                    pass = Console.ReadLine();
                    Data_output(pass, stream);

                    Console.WriteLine("Введите имя:");
                    name = Console.ReadLine();
                    Data_output(name, stream);

                    Console.WriteLine("Введите возраст:");
                    age = Console.ReadLine();
                    Data_output(age, stream);

                    Data_input(response, bytesRead, stream);

                    break;
            }
            response.Clear();
            if (text.Equals("/Stop")) break;
        }
    }
    static void Data_input(List<byte> response, int bytesRead, NetworkStream stream)
    {
        while ((bytesRead = stream.ReadByte()) != '\n')
        {
            response.Add((byte)bytesRead);
        }
        string data = Encoding.UTF8.GetString(response.ToArray());
        response.Clear();
        Console.WriteLine($"сервер: {data}");
    }
    static async Task Data_output(string text, NetworkStream stream)
    {
        byte[] data = Encoding.UTF8.GetBytes(text + '\n');
        await stream.WriteAsync(data);
        Console.WriteLine($"Клиент отправил {text}");


    }
}