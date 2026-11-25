using System;
using System.Collections.Generic;

namespace Buying_bouquets.Models;

public partial class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BouquetId { get; set; }

    public int Quantity { get; set; }

    public virtual Bouquet Bouquet { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
