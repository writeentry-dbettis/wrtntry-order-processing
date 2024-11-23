using System;
using BatchProcessing.Common.Interfaces;

namespace BatchProcessing.Common.Models;

public class Order : INameableEntity
{
    public string Id { get; set;} = string.Empty;
    public string? PONumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Tax { get; set; }
    public DateTime CreatedDate { get; set; }

    public string Name() => $"{PONumber} ({Id})";
}
