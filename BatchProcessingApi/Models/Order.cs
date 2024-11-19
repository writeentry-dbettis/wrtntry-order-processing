using System;

namespace BatchProcessingApi.Models;

public class Order : INameableEntity
{
    public int Id { get; set;}
    public string? PONumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Tax { get; set; }
    public DateTime CreatedDate { get; set; }

    public string Name() => $"{PONumber} ({Id})";
}

public interface INameableEntity
{
    string Name();
}
