using System;
using System.Collections.Generic;

namespace Buying_bouquets.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string RecipientPhone { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
