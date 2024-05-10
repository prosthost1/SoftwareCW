using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SoftwareCW.Views
{
    public partial class AlternativesPage : ContentPage
    {
        public AlternativesPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var unevaluatedNodes = App.GetUnevaluatedNodes();
            if (unevaluatedNodes.node != null)
            {
                WarningLabel.Text = $"'{unevaluatedNodes.node.Name}' and {unevaluatedNodes.count} more nodes require evaluation.";
                DescriptionLabel.Text = "Ratings of the alternatives are not available:";
                submitButton.IsEnabled = false;
            }
            else
            {
                WarningLabel.Text = "";
                DescriptionLabel.Text = "Ratings of the alternatives:";
                App.CalculateGlobalPriorities();
                submitButton.IsEnabled = true;
            }
            PopulateListView();
        }

        private void PopulateListView()
        {
            var alternatives = App.FindAlternatives().OrderByDescending(node => node.GlobalPriority).ToList();
            if (App.GetUnevaluatedNodes().node != null)
            {
                foreach (var alternative in alternatives)
                {
                    alternative.GlobalPriority = 0;
                }
            }
            AlternativesListView.ItemsSource = alternatives;
        }

        void submitButton_Clicked(System.Object sender, System.EventArgs e)
        {
            var alternatives = App.FindAlternatives().OrderByDescending(node => node.GlobalPriority).ToList();
            var alternativesData = alternatives.Select(a => new { a.Name, GlobalPriority = a.FormattedGlobalPriority }).ToList();
            string jsonAlternatives = JsonConvert.SerializeObject(alternativesData, Formatting.Indented);

            string[] recipients = { "grigory.pishchulov@manchester.ac.uk" };
            string subject = $"AHP results for {App.nodes[0].Name}";
            string body = jsonAlternatives;

            EmailMessage message;
            try
            {
                message = new EmailMessage(subject, body, recipients);
                Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException ex)
            {
                DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
