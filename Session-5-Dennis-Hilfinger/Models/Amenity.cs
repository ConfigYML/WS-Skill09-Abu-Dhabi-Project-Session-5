using System;
using System.Collections.Generic;

namespace Session_5_Dennis_Hilfinger;

public partial class Amenity
{
    public int Id { get; set; }

    public string Service { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<AmenitiesTicket> AmenitiesTickets { get; set; } = new List<AmenitiesTicket>();

    public virtual ICollection<CabinType> CabinTypes { get; set; } = new List<CabinType>();
}
