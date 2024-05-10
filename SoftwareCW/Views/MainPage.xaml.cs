using Xamarin.Forms;

namespace SoftwareCW.Views
{
    public partial class MainPage : ContentPage
    {
        public MainMenuItem[] mainMenuItems;
        public MainPage()
        {
            InitializeComponent();
            mainMenuItems = new MainMenuItem[2];
            mainMenuItems[0] = new MainMenuItem { Title = "Hierarchy", Description = "Browse and evaluate" };
            mainMenuItems[1] = new MainMenuItem { Title = "Decision alternatives", Description = "View rating scores" };
            ListView1.ItemsSource = mainMenuItems;

        }
        void ListView1_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        { 
            MainMenuItem item = (MainMenuItem)e.Item;
            if (item.Title == "Hierarchy")
            {
                HierarchyPage p = new HierarchyPage(App.nodes[0]);
                Navigation.PushAsync(p);
            }
            else if (item.Title == "Decision alternatives")
            {
                AlternativesPage p = new AlternativesPage();
                Navigation.PushAsync(p);
            }
        }

    }
    public class MainMenuItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

}