
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel.VoiceCommands;

namespace Session_5_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage
    {
        private bool FlightNumberSearchType = true;
        public MainPage()
        {
            InitializeComponent();
        }

        private async void ChangeSearchType(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton) sender;
            if (rb.Value.ToString() == "FlightNumber")
            {
                FlightNumberSearchType = true;
                FlightNumberInput.IsEnabled = true;
                FlightDatePicker.IsEnabled = true;
                FromDatePicker.IsEnabled = false;
                ToDatePicker.IsEnabled = false;
            } else
            {
                FlightNumberSearchType = false;
                FlightNumberInput.IsEnabled = false;
                FlightDatePicker.IsEnabled = false;
                FromDatePicker.IsEnabled = true;
                ToDatePicker.IsEnabled = true;
            }
        }

        private async void LoadReport(object sender, EventArgs e)
        {
            using(var db = new AirlineContext())
            {
                ReportGrid.Clear();
                if (String.IsNullOrEmpty(FlightNumberInput.Text))
                {
                    await DisplayAlert("Info", "Please enter a flight number.", "Ok");
                    return;
                }
                IQueryable<Schedule> schedules;

                if (FlightNumberSearchType)
                {
                    DateOnly flyDate = DateOnly.FromDateTime(FlightDatePicker.Date);

                    schedules = db.Schedules.Where(s => s.FlightNumber == FlightNumberInput.Text && s.Date == flyDate);
                } else
                {
                    DateOnly fromDate = DateOnly.FromDateTime(FromDatePicker.Date);
                    DateOnly toDate = DateOnly.FromDateTime(ToDatePicker.Date);

                    schedules = db.Schedules.Where(s => s.Date >= fromDate && s.Date <= toDate);
                }

                if (schedules.Count() == 0)
                {
                    await DisplayAlert("Info", "No flights found using these criteria.", "Ok");
                    return;
                }
                var amenities = db.AmenitiesTickets
                                        .Include(at => at.Ticket)
                                        .ThenInclude(t => t.Schedule)
                                        .Include(at => at.Amenity)
                                        .Where(at => schedules.Contains(at.Ticket.Schedule));
                var economyId = db.CabinTypes.FirstOrDefault(ct => ct.Name == "Economy").Id;
                var businessId = db.CabinTypes.FirstOrDefault(ct => ct.Name == "Business").Id;
                var firstClassId = db.CabinTypes.FirstOrDefault(ct => ct.Name == "First Class").Id;
                var economy = amenities.Where(at => at.Ticket.CabinTypeId == economyId);
                var business = amenities.Where(at => at.Ticket.CabinTypeId == businessId);
                var firstClass = amenities.Where(at => at.Ticket.CabinTypeId == firstClassId);

                ColumnDefinitionCollection colDefs = ReportGrid.ColumnDefinitions = new ColumnDefinitionCollection(); 
                for (int i = 0; i <= amenities.Count(); i++)
                {
                    colDefs.Add(new ColumnDefinition());
                }
                ReportGrid.ColumnDefinitions = colDefs;

                int index = 1;
                List<int> valueOrder = new List<int> { -1 };

                foreach (var a in amenities)
                {
                    if (valueOrder.Any(v => v == a.AmenityId))
                    {
                        continue;
                    }
                    valueOrder.Add(a.AmenityId);

                    Label headerLabel = new Label();
                    headerLabel.Text = a.Amenity.Service;
                    Grid.SetColumn(headerLabel, index++);
                    ReportGrid.Add(headerLabel);
                }

                Label econLabel = new Label();
                econLabel.Text = "Economy";
                Grid.SetRow(econLabel, 1);
                ReportGrid.Add(econLabel);

                Label businessLabel = new Label();
                businessLabel.Text = "Business";
                Grid.SetRow(businessLabel, 2);
                ReportGrid.Add(businessLabel);

                Label firstClassLabel = new Label();
                firstClassLabel.Text = "First Class";
                Grid.SetRow(firstClassLabel, 3);
                ReportGrid.Add(firstClassLabel);

                for (int i = 1; i < valueOrder.Count(); i++)
                {
                    Label econCountLabel = new Label();
                    econCountLabel.Text = economy.Where(e => e.AmenityId == valueOrder[i]).Count().ToString();
                    Grid.SetColumn(econCountLabel, i);
                    Grid.SetRow(econCountLabel, 1);
                    ReportGrid.Add(econCountLabel);

                    Label businessCountLabel = new Label();
                    businessCountLabel.Text = business.Where(e => e.AmenityId == valueOrder[i]).Count().ToString();
                    Grid.SetColumn(businessCountLabel, i);
                    Grid.SetRow(businessCountLabel, 2);
                    ReportGrid.Add(businessCountLabel);

                    Label firstClassCountLabel = new Label();
                    firstClassCountLabel.Text = firstClass.Where(e => e.AmenityId == valueOrder[i]).Count().ToString();
                    Grid.SetColumn(firstClassCountLabel, i);
                    Grid.SetRow(firstClassCountLabel, 3);
                    ReportGrid.Add(firstClassCountLabel);
                }
                
                
            }
        }

    }

}
