using System;
using System.Collections.Generic;

namespace Buying_bouquets.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int BouquetId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtOrder { get; set; }

    public virtual Bouquet Bouquet { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
