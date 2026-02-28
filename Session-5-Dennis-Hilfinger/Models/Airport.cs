using System;
using System.Collections.Generic;

namespace Session_5_Dennis_Hilfinger;

public partial class Airport
{
    public int Id { get; set; }

    public int CountryId { get; set; }

    public string Iatacode { get; set; } = null!;

    public string? Name { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<Route> RouteArrivalAirports { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteDepartureAirports { get; set; } = new List<Route>();
}
