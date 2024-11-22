// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using BatchProcessing.Common.Models;

Console.WriteLine("Hello, World!");

string jsonMessage = "{\"type\":\"BatchProcessing.Common.Models.Order\",\"environment\":\"development\",\"data\":{\"id\":2732919,\"poNumber\":\"Micah Jones\",\"totalAmount\":270.8300,\"tax\":22.9300,\"createdDate\":\"2024-11-14T21:26:51.247\"}}";

using (var jsonDocument = JsonDocument.Parse(jsonMessage))
{
    var typeName = jsonDocument.RootElement.GetProperty("type");
    var data = jsonDocument.RootElement.GetProperty("data");

    var order = data.Deserialize<Order>(new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    Console.WriteLine(order?.Name());
}

Console.WriteLine("");
