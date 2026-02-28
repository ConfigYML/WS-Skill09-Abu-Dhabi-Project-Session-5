
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace Session_5_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage
    {
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
            using(var db = new AirlineContext())
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
                foreach(var t in tickets)
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

                for(int i = 0; i < columnCount; i++)
                {
                    HorizontalStackLayout layout = new HorizontalStackLayout();
                    Grid.SetColumn(layout, i / 4);
                    Grid.SetRow(layout, i % 4);
                    
                    CheckBox check = new CheckBox();
                    
                    layout.Add(check);

                    Label checkLabel = new Label();
                    /*if (int.TryParse(amenities.ElementAt(0).Price, out int price) {

                    }*/
                    checkLabel.Text = $"{amenities.ElementAt(i).Service} (${int.Parse(amenities.ElementAt(i).Price.ToString())})";
                    layout.Add(checkLabel);

                    AmenitiesGrid.Add(layout);
                }
            }
        }

        private async void Save(object sender, EventArgs e)
        {

        }
        private async void Exit(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }
    }

}
