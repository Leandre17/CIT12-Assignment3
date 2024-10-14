using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

TcpListener listener = new(IPAddress.Loopback, 5000);
listener.Start();
Console.WriteLine("Server is listening on port 5000...");

List<Category> categories = new List<Category>
    {
        new Category { cid = 1, name = "Beverages" },
        new Category { cid = 2, name = "Condiments" },
        new Category { cid = 3, name = "Confections" }
};

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

Response HandleRequest(Request request) {
    if (request == null) return new Response { status = "4 missing method", body = "" };
    string[] pathParts = request.path.Split('/');
    int cid = -1;
    switch (request.method) {
        case "read":
            if (request.path == "/api/categories")
                return new Response { status = "1 Ok", body = JsonSerializer.Serialize(categories) };
            else if (request.path.StartsWith("/api/categories/")) {
                pathParts = request.path.Split('/');
                if (pathParts.Length == 3 && int.TryParse(pathParts[2], out cid)) {
                    var category = categories.Find(c => c.cid == cid);
                    if (category != null)
                        return new Response { status = "1 Ok", body = JsonSerializer.Serialize(category) };
                    else
                        return new Response { status = "5 Not Found", body = "" };
                    
                }
            }
            return new Response { status = "4 Bad Request", body = "invalid path" };
        case "create":
            if (string.IsNullOrEmpty(request.body))
                return new Response { status = "4 missing body", body = "" };
            else if (request.path != "/api/categories") {
                return new Response { status = "4 Bad Request", body = "invalid path" };
            }
            var newCategory = JsonSerializer.Deserialize<Category>(request.body);
            if (newCategory == null || string.IsNullOrEmpty(newCategory.name)) {
                return new Response { status = "4 Bad Request", body = "invalid body" };
            }
            newCategory.cid = categories.Count + 1;
            categories.Add(newCategory);
            return new Response { status = "2 Created", body = JsonSerializer.Serialize(newCategory)};
        case "update":
            if (string.IsNullOrEmpty(request.body))
                return new Response { status = "4 missing body", body = "" };
            else if (string.IsNullOrEmpty(request.path) || !request.path.StartsWith("/api/categories/"))
                return new Response { status = "4 Bad Request", body = "missing or invalid path" };
            pathParts = request.path.Split('/');
            if (pathParts.Length == 3 && int.TryParse(pathParts[2], out cid)) {
                var category = categories.Find(c => c.cid == cid);
                if (category != null) {
                    var updatedCategory = JsonSerializer.Deserialize<Category>(request.body);
                    if (updatedCategory != null && !string.IsNullOrEmpty(updatedCategory.name)) {
                        category.name = updatedCategory.name;
                        return new Response { status = "3 Updated", body = "" };
                    }
                } else
                    return new Response { status = "5 Not Found", body = "" };
            }
            return new Response { status = "4 Bad Request", body = "invalid path or body" };
        case "delete":
            if (string.IsNullOrEmpty(request.body))
                return new Response { status = "4 missing body", body = "" };
            else if (string.IsNullOrEmpty(request.path) || !request.path.StartsWith("/api/categories/"))
                return new Response { status = "4 Bad Request", body = "missing or invalid path" };
            pathParts = request.path.Split('/');
            if (pathParts.Length == 3 && int.TryParse(pathParts[2], out cid)) {
                var category = categories.Find(c => c.cid == cid);
                if (category != null) {
                    categories.Remove(category);
                    return new Response { status = "1 Ok", body = "" };
                } else
                    return new Response { status = "5 Not Found", body = "" };
            }
            return new Response { status = "4 Bad Request", body = "invalid path" };
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
