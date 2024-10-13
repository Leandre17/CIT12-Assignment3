using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

TcpListener listener = new(IPAddress.Loopback, 5000);
listener.Start();
Console.WriteLine("Server is listening on port 5000...");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    NetworkStream stream = client.GetStream();
    byte[] buffer = new byte[1024];
    int byteCount = stream.Read(buffer);
    string requestString = Encoding.UTF8.GetString(buffer, 0, byteCount);
    Console.WriteLine(requestString);
    Request request = JsonSerializer.Deserialize<Request>(requestString);

    Response response = HandleRequest(request);
    string msg = JsonSerializer.Serialize(response);
    Console.WriteLine(msg);
    var buffer2 = Encoding.UTF8.GetBytes(msg);
    stream.Write(buffer2);
}

static Response HandleRequest(Request request) {
    if (request == null) return new Response { status = "4 missing method", body = "" };
    switch (request.method) {
        case "read":
            return new Response { status = "1 OK", body = "" };
        case "create":
            if (string.IsNullOrEmpty(request.body))
                return new Response { status = "4 missing body", body = "" };
            else if (request.path != "/api/categories") {
                return new Response { status = "4 Bad Request", body = "invalid path" };
            }
            return new Response { status = "2 Created", body = "" };
        case "update":
            if (string.IsNullOrEmpty(request.body))
                return new Response { status = "4 missing body", body = "" };
            if (string.IsNullOrEmpty(request.path) || !request.path.StartsWith("/api/categories/"))
                return new Response { status = "4 Bad Request", body = "missing or invalid path" };
            return new Response { status = "3 Updated", body = "" };
        case "delete":
            if (string.IsNullOrEmpty(request.body))
                return new Response { status = "4 missing body", body = "" };
            if (string.IsNullOrEmpty(request.path) || !request.path.StartsWith("/api/categories/"))
                return new Response { status = "4 Bad Request", body = "missing or invalid path" };
            return new Response { status = "1 OK", body = "" };
        case "echo":
            return new Response { status = "1 OK", body = request.body };
        default:
            return new Response { status = "4 illegal method", body = "" };
    }
}



public class Request
{
    public string? method { get; set; }
    public string? path { get; set; }
    public string? date { get; set; }
    public string? body { get; set; }
}

// Response class to format outgoing responses
public class Response
{
    public string? status { get; set; }
    public string? body { get; set; }
}

// Category class for handling category objects
public class Category
{
    public int cid { get; set; }
    public string? name { get; set; }
}
