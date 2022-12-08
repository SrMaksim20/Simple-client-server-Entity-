using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Server
{

    public static async Task Main(string[] str)
    {
        var tcpListener = new TcpListener(IPAddress.Any, 8888);

        try
        {
            tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();

                new Thread(async () => await ProcessClientAsync(tcpClient)).Start();
            }
        }
        finally
        {
            tcpListener.Stop();
        }
        async Task ProcessClientAsync(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            var response = new List<byte>();
            int bytesRead = 10;
            byte[] data;
            string log;
            string pass;
            string name;
            int age;
            bool b;
            Console.WriteLine($"Клиент {tcpClient.Client.RemoteEndPoint} подключился");
            while (true)
            {
                while ((bytesRead = stream.ReadByte()) != '\n')
                {
                    response.Add((byte)bytesRead);
                }
                var num = Encoding.UTF8.GetString(response.ToArray());
                Console.WriteLine(num);
                response.Clear();
                switch (num)
                {
                    case "/Sing_in":
                        b = false;
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            log = Data_input(response, bytesRead, stream);
                            pass = Data_input(response, bytesRead, stream);

                            foreach (User users in db.Users.ToList())
                            {
                                Console.WriteLine($"DB{users.Login} == {log}");
                                Console.WriteLine($"DB{users.Password} == {pass}");

                                if (log == users.Login && pass == users.Password)
                                {
                                    Data_output(stream, $"Добро пожаловать {users.Name}\n");
                                    b = true;
                                }
                            }
                            if (b == false)
                            {
                                Data_output(stream, $"Пользователь не найден");
                            }
                        }
                        break;
                    case "/Sing_up":
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            b = false;
                            log = Data_input(response, bytesRead, stream);
                            pass = Data_input(response, bytesRead, stream);
                            name = Data_input(response, bytesRead, stream);
                            age = int.Parse(Data_input(response, bytesRead, stream));

                            foreach (User users in db.Users.ToList())
                            {
                                Console.WriteLine($"DB{users.Login} == {log}");
                                Console.WriteLine($"DB{users.Password} == {pass}");

                                if (log == users.Login)
                                {
                                    b = true;
                                }
                            }
                            if (b == true)
                            {
                                Data_output(stream, $"Такой логин уже занят");
                            }
                            else
                            {
                                User user = new User { Login = log, Password = pass, Name = name, Age = age };
                                db.Users.Add(user);
                                db.SaveChanges();
                                Data_output(stream, $"Пользователь успешно зарегестрилован!");
                            }
                        }
                        break;
                }
                if (num == "/Stop")
                {
                    Console.WriteLine($"Клиент {tcpClient.Client.RemoteEndPoint} вышел");
                    break;
                }
                response.Clear();
            }
            tcpClient.Close();
        }
    }
    static string Data_input(List<byte> response, int bytesRead, NetworkStream stream)
    {
        while ((bytesRead = stream.ReadByte()) != '\n')
        {
            response.Add((byte)bytesRead);
        }
        string data = Encoding.UTF8.GetString(response.ToArray());
        response.Clear();
        Console.WriteLine($"клиент: ({data})");
        return data;
    }
    static async Task Data_output(NetworkStream stream, string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text + '\n');
        await stream.WriteAsync(data);
        Console.WriteLine($"Сервер отправил {text}");
    }
}
class User
{
    public int Id { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}

class ApplicationContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=DB.db");
    }
}