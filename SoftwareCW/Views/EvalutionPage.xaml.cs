using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Linq;

namespace SoftwareCW.Views
{	
	public partial class EvalutionPage : ContentPage
	{
        private Node _node;
        private PreferenceMatrix preferenceMatrix;
        public List<Preference> Preferences { get; set; } = new List<Preference>();

        public EvalutionPage (Node node)
		{
			InitializeComponent ();
            this.Title = node.Name;
            _node = node;
            this.BindingContext = this;
            var childNodes = _node.Children.Select(App.FindNodeByName).ToList();
            preferenceMatrix = new PreferenceMatrix(childNodes.Select(c => c.Name).ToList());
        }
        private void GeneratePreferences(List<string> children)
        {
            Preferences.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                for (int j = i + 1; j < children.Count; j++)
                {
                    Preferences.Add(new Preference
                    {
                        Item1 = children[i],
                        Item2 = children[j],
                        IsItem2Preferred = false,
                        Strength = 0
                    });
                }
            }
            preferencesListView.ItemsSource = Preferences; // Make sure to update the UI
        }
        private void InitializePreferences()
        {
            List<string> childNodeNames = _node.Children.Select(child => App.FindNodeByName(child)?.Name).Where(name => name != null).ToList();
            if (!LoadPreferences() || Preferences.Count == 0) // Check if preferences are empty even after loading
            {
                GeneratePreferences(childNodeNames);
            }
        }
        void saveButton_Clicked(System.Object sender, System.EventArgs e)
        {
            UpdateMatrixWithPreferences();
            double[] priorities = preferenceMatrix.CalculatePriorities();
            UpdateNodePriorities(priorities);
            SavePreferences();
            Navigation.PopAsync();
        }
        void UpdateMatrixWithPreferences()
        {
            var childNodes = _node.Children.Select(App.FindNodeByName).Where(n => n != null).ToList();
            foreach (var pref in Preferences)
            {
                int index1 = childNodes.FindIndex(n => n.Name == pref.Item1);
                int index2 = childNodes.FindIndex(n => n.Name == pref.Item2);
                if (index1 != -1 && index2 != -1)
                {
                    double value = pref.IsItem2Preferred ? 1.0 / (pref.Strength + 1) : (pref.Strength + 1);
                    Console.WriteLine($"Updating preference for {pref.Item1} over {pref.Item2} with value {value}");
                    preferenceMatrix.UpdatePreference(index1, index2, value);
                }
            }
        }

        void UpdateNodePriorities(double[] priorities)
        {
            var childNodes = _node.Children.Select(App.FindNodeByName).Where(n => n != null).ToList();
            if (childNodes.Count != priorities.Length)
                throw new InvalidOperationException("Mismatch between child nodes and calculated priorities");

            for (int i = 0; i < childNodes.Count; i++)
            {
                childNodes[i].Priorities[_node.Name] = priorities[i];  // Store priority with respect to the parent node
            }
        }
        private void SavePreferences()
        {
            var preferencesJson = JsonConvert.SerializeObject(Preferences);
            string key = $"Preferences_{_node.Name}"; // Unique key for each node
            Application.Current.Properties[key] = preferencesJson;
            Application.Current.SavePropertiesAsync();
        }
        private bool LoadPreferences()
        {
            string key = $"Preferences_{_node.Name}";
            if (Application.Current.Properties.ContainsKey(key))
            {
                var preferencesJson = Application.Current.Properties[key].ToString();
                Preferences = JsonConvert.DeserializeObject<List<Preference>>(preferencesJson);
                if (Preferences == null || !Preferences.Any())
                {
                    return false; // If no preferences or empty, return false to trigger default generation
                }
                preferencesListView.ItemsSource = Preferences;
                return true;
            }
            return false;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitializePreferences();
            preferencesListView.ItemsSource = null; // Clear existing binding
            preferencesListView.ItemsSource = Preferences; // Refresh with current preferences
        }
public class PreferenceMatrix
{
    public double[,] Matrix { get; private set; }
    public List<string> Items { get; private set; }

    public PreferenceMatrix(List<string> items)
    {
        Items = items;
        int size = items.Count;
        Matrix = new double[size, size];

        // Initialize matrix diagonal to 1
        for (int i = 0; i < size; i++)
        {
            Matrix[i, i] = 1;
        }
    }

    public void UpdatePreference(int rowIndex, int colIndex, double value)
    {
        Matrix[rowIndex, colIndex] = value;
        Matrix[colIndex, rowIndex] = 1 / value;
    }

    public double[] CalculatePriorities()
    {
        int size = Items.Count;
        double[] columnSums = new double[size];
        double[] priorities = new double[size];

        // Step 1: Calculate column sums
        for (int col = 0; col < size; col++)
        {
            double sum = 0;
            for (int row = 0; row < size; row++)
            {
                sum += Matrix[row, col];
            }
            columnSums[col] = sum;
        }

        // Step 2: Normalize the matrix and calculate row sums
        double[] rowSums = new double[size];
        for (int row = 0; row < size; row++)
        {
            double rowSum = 0;
            for (int col = 0; col < size; col++)
            {
                double normalizedValue = Matrix[row, col] / columnSums[col];
                rowSum += normalizedValue;
            }
            rowSums[row] = rowSum;
        }

        // Step 3: Normalize row sums to derive priorities
        double totalSum = 0;
        foreach (var sum in rowSums)
        {
            totalSum += sum;
        }

        for (int i = 0; i < size; i++)
        {
            priorities[i] = rowSums[i] / totalSum;
        }

        return priorities;
    }
}

    }
}

