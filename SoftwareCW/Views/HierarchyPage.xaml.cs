using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using Xamarin.Essentials;

namespace SoftwareCW.Views
{
    public partial class HierarchyPage : ContentPage
    {
        private Node currentNode;
        public List<Node> ChildNodes { get; set; }

        public HierarchyPage(Node node)
        {
            InitializeComponent();
            currentNode = node;
            this.Title = node.Name;
            InitializeChildNodes();
            this.BindingContext = currentNode;
            HierarchyListView.ItemsSource = ChildNodes;
            evaluateButton.IsVisible = ChildNodes.Any();
        }

        private void InitializeChildNodes()
        {
            if (currentNode.Children != null && currentNode.Children.Any())
            {
                ChildNodes = currentNode.Children.Select(App.FindNodeByName).Where(childNode => childNode != null).ToList();
                foreach (var child in ChildNodes)
                {
                    double priority;
                    if (child.Priorities.TryGetValue(currentNode.Name, out priority))
                        child.DisplayPriority = $"{priority:P2}";
                    else
                        child.DisplayPriority = "";
                }
            }
            else
            {
                ChildNodes = new List<Node>();
            }
        }

        private void HierarchyListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            Node selectedNode = e.SelectedItem as Node;
            if (selectedNode != null)
            {
                Navigation.PushAsync(new HierarchyPage(selectedNode));
            }

            ((ListView)sender).SelectedItem = null; // Reset the selected item
        }

        void evaluateButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new EvalutionPage(currentNode));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshChildNodes();
        }

        private void RefreshChildNodes()
        {
            InitializeChildNodes();
            HierarchyListView.ItemsSource = null;
            HierarchyListView.ItemsSource = ChildNodes;
            evaluateButton.IsVisible = ChildNodes.Any(); 
        }
    }
}
