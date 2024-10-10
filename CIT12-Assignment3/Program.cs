using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

TcpListener listener = new TcpListener(IPAddress.Any, 5000);
listener.Start();
Console.WriteLine("Server is listening on port 5000...");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    NetworkStream stream = client.GetStream();
    byte[] buffer = new byte[1024];
    int byteCount = stream.Read(buffer);
    string requestString = Encoding.UTF8.GetString(buffer, 0, byteCount);

    Request request = JsonSerializer.Deserialize<Request>(requestString);

    Response response = new Response { status = "4 Bad Request", body = "missing method" };
    string msg = JsonSerializer.Serialize(response);
    var buffer2 = Encoding.UTF8.GetBytes(msg);
    stream.Write(buffer2);
}






public class Request
{
    public string method { get; set; }
    public string path { get; set; }
    public string date { get; set; }
    public string body { get; set; }
}

// Response class to format outgoing responses
public class Response
{
    public string status { get; set; }
    public string body { get; set; }
}

// Category class for handling category objects
public class Category
{
    public int cid { get; set; }
    public string name { get; set; }
}
