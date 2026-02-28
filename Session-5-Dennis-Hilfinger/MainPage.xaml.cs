
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel.VoiceCommands;

namespace Session_5_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage
    {
        List<AmenityDTO> amenityIds = new List<AmenityDTO>();
        int currentTicketId;
        public MainPage()
        {
            InitializeComponent();
        }

        private async void SearchForFlights(object sender, EventArgs e)
        {
            string? bookingRef = BookingRefInput.Text;
            if (String.IsNullOrEmpty(bookingRef))
            {
                await DisplayAlert("Info", "Please enter a booking reference number to proceed.", "Ok");
                return;
            }
            FlightPicker.Items.Clear();
            AmenitiesGrid.Clear();
            using (var db = new AirlineContext())
            {
                if (!db.Tickets.Any(t => t.BookingReference == bookingRef.ToUpper()))
                {
                    await DisplayAlert("Info", "No tickets were found under this booking reference number. Please check if you entered it correctly.", "Ok");
                    return;
                }
                var tickets = db.Tickets
                    .Where(t => t.BookingReference == bookingRef.ToUpper())
                    .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                    .ThenInclude(r => r.ArrivalAirport)
                    .Include(t => t.Schedule)
                    .ThenInclude(s => s.Route)
                    .ThenInclude(r => r.DepartureAirport);
                foreach (var t in tickets)
                {
                    string flightNumber = t.Schedule.FlightNumber;
                    string departureAirport = t.Schedule.Route.DepartureAirport.Iatacode;
                    string arrivalAirport = t.Schedule.Route.ArrivalAirport.Iatacode;
                    DateOnly takeoffDate = t.Schedule.Date;
                    TimeOnly takeoffTime = t.Schedule.Time;
                    string flightString = String.Format("{0}, {1}, {2}, {3}, {4}",
                                                        flightNumber,
                                                        departureAirport,
                                                        arrivalAirport,
                                                        takeoffDate,
                                                        takeoffTime);
                    FlightPicker.Items.Add(flightString);
                }
            }
        }
        private async void LoadAmenities(object sender, EventArgs e)
        {
            FullnameLabel.Text = "";
            PassportNumberLabel.Text = "";
            CabinTypeLabel.Text = "";
            AmenitiesGrid.Clear();

            using (var db = new AirlineContext())
            {
                if (FlightPicker.SelectedItem == null)
                {
                    await DisplayAlert("Info", "Please select a flight.", "Ok");
                    return;
                }
                var flightString = FlightPicker.SelectedItem.ToString();
                string flightNumber = flightString.Split(',')[0].Trim();

                var ticket = db.Tickets
                                    .Include(t => t.Schedule)
                                    .FirstOrDefault(t => t.Schedule.FlightNumber == flightNumber);
                currentTicketId = ticket.Id;
                var cabinType = db.CabinTypes
                                    .Include(ct => ct.Amenities)
                                    .FirstOrDefault(ct => ct.Id == ticket.CabinTypeId);

                FullnameLabel.Text = $"{ticket.Firstname} {ticket.Lastname}";
                PassportNumberLabel.Text = ticket.PassportNumber;
                CabinTypeLabel.Text = cabinType.Name;

                var amenities = cabinType.Amenities;
                var columnCount = amenities.Count() / 4;
                if ((amenities.Count() % 4) != 0)
                {
                    columnCount++;
                }

                ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();
                for (int i = 0; i < columnCount; i++)
                {
                    columnDefinitions.Add(new ColumnDefinition());
                }
                AmenitiesGrid.ColumnDefinitions = columnDefinitions;
                amenityIds.Clear();

                for (int i = 0; i < amenities.Count(); i++)
                {
                    var currentAmenity = amenities.ElementAt(i);

                    Grid checkGrid = new Grid()
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection() {
                            new ColumnDefinition(),
                            new ColumnDefinition()
                        }
                    };
                    Grid.SetColumn(checkGrid, i / 4);
                    Grid.SetRow(checkGrid, i % 4);

                    CheckBox check = new CheckBox();
                    Grid.SetColumn(check, 0);
                    check.HorizontalOptions = LayoutOptions.End;
                    check.CheckedChanged += UpdateAmounts;
                    if (db.AmenitiesTickets.Any(at => at.TicketId == ticket.Id && at.AmenityId == currentAmenity.Id))
                    {
                        check.IsChecked = true;
                    }


                    Label checkLabel = new Label();
                    Grid.SetColumn(checkLabel, 1);
                    var price = "Free";
                    if (currentAmenity.Price != decimal.Zero)
                    {
                        price = decimal.ToInt32(currentAmenity.Price).ToString();
                    } else
                    {
                        check.IsEnabled = false;
                        checkLabel.TextColor = Colors.Grey;
                    }
                    checkLabel.Text = $"{currentAmenity.Service} (${price})";

                    amenityIds.Add(new AmenityDTO()
                    {
                        Id = currentAmenity.Id,
                        Price = currentAmenity.Price,
                        checkBox = check
                    });

                    checkGrid.Add(check);
                    checkGrid.Add(checkLabel);

                    AmenitiesGrid.Add(checkGrid);
                }
            }
        }

        private async void UpdateAmounts(object sender, EventArgs e)
        {
            using (var db = new AirlineContext())
            {
                var selectedAmenities = db.AmenitiesTickets
                                            .Include(at => at.Ticket)
                                            .Include(at => at.Amenity)
                                            .Where(at => at.Ticket.Id == currentTicketId);
                decimal priorAmount = selectedAmenities.Sum(at => at.Amenity.Price);

                decimal totalAmount = decimal.Zero; 
                foreach (var am in amenityIds)
                {
                    // Am added
                    if (am.checkBox.IsChecked && !selectedAmenities.Any(a => a.AmenityId == am.Id))
                    {
                        totalAmount += am.Price;
                    } else if (am.checkBox.IsChecked == false && selectedAmenities.Any(a => a.AmenityId == am.Id)) // Am removed
                    {
                        totalAmount -= am.Price;
                    }
                    
                }

                var doubleValue = decimal.ToDouble(totalAmount);
                var taxValue = doubleValue * 0.05;

                SelectedAmountLabel.Text = $"$ {doubleValue}";
                TaxAmountLabel.Text = $"$ {taxValue}";
                TotalAmountLabel.Text = $"$ {(doubleValue + taxValue)}";
            }
            
        }

        private async void Save(object sender, EventArgs e)
        {
            using(var db = new AirlineContext())
            {
                var selectedAmenities = db.AmenitiesTickets
                                            .Include(at => at.Ticket)
                                            .Include(at => at.Amenity)
                                            .Where(at => at.Ticket.Id == currentTicketId);
                List<Amenity> amenitiesToSave = new List<Amenity>();
                foreach(var am in amenityIds)
                {
                    if (am.checkBox.IsChecked)
                    {
                        amenitiesToSave.Add(db.Amenities.FirstOrDefault(a => a.Id == am.Id));
                    }
                }
                // Removing unselected amenities
                foreach(var select in selectedAmenities)
                {
                    if (!amenitiesToSave.Any(ats => ats.Id == select.AmenityId))
                    {
                        db.AmenitiesTickets.Remove(select);
                    }
                }
                await db.SaveChangesAsync();
                // Adding new amenities
                foreach(var ats in amenitiesToSave)
                {
                    if (!db.AmenitiesTickets.Any(at => at.Amenity.Id == ats.Id)) {
                        db.AmenitiesTickets.Add(new AmenitiesTicket()
                        {
                            TicketId = currentTicketId,
                            AmenityId = ats.Id,
                            Price = ats.Price
                        });
                    }
                }
                await db.SaveChangesAsync();
            }
            
        }


        private async void Exit(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }

        public class AmenityDTO
        {
            public int Id { get; set; }
            public decimal Price { get; set; }
            public CheckBox checkBox { get; set; }
        }
    }

}
