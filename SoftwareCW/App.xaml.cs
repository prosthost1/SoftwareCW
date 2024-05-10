using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.IO;
using System.Net;

using Xamarin.Forms.Xaml;
using System.Linq;

namespace SoftwareCW
{
    public partial class App : Application
    {
        public static Node[] nodes;
        public App ()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new Views.MainPage());
        }

        public static void CalculateGlobalPriorities()
        {
            if (App.nodes != null && App.nodes.Length > 0)
            {
                Node topNode = App.nodes[0];
                SetGlobalPriority(topNode, 1.0);
            }
        }

        private static void SetGlobalPriority(Node node, double parentPriority)
        {
            Console.WriteLine($"Setting Global Priority for {node.Name} to {parentPriority}");
            node.GlobalPriority = parentPriority;

            if (node.Children != null)
            {
                foreach (var childName in node.Children)
                {
                    Node childNode = FindNodeByName(childName);
                    Console.WriteLine($"FindNodeByName result for {childName}: {childNode != null}");

                    if (childNode != null)
                    {
                        double childGlobalPriority = 0;

                        // Iterate over all parent nodes of the child node
                        foreach (var parentNodeName in childNode.Priorities.Keys)
                        {
                            Node parentNode = FindNodeByName(parentNodeName);

                            if (parentNode != null && childNode.Priorities.TryGetValue(parentNodeName, out double localPriority))
                            {
                                // Add the product of the local priority and the global priority of the parent node to the global priority of the child node
                                childGlobalPriority += localPriority * parentNode.GlobalPriority;
                            }
                        }

                        Console.WriteLine($"Processing Child: {childName}, Global Priority: {childGlobalPriority}");
                        SetGlobalPriority(childNode, childGlobalPriority);
                    }
                    else
                    {
                        Console.WriteLine($"Child node not found for {childName}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"No children for node {node.Name}");
            }
        }
        public static (Node node, int count) GetUnevaluatedNodes()
        {
            List<Node> unevaluatedNodes = new List<Node>();
            foreach (var node in nodes)
            {
                if (node.Children != null)
                {
                    foreach (var childName in node.Children)
                    {
                        Node childNode = FindNodeByName(childName);
                        if (childNode != null && !childNode.Priorities.ContainsKey(node.Name))
                        {
                            unevaluatedNodes.Add(node);
                            break;
                        }
                    }
                }
            }
            Node firstNode = unevaluatedNodes.FirstOrDefault();
            int count = unevaluatedNodes.Count - 1;
            return (firstNode, count);
        }
        protected override void OnStart ()
        {
            string n = "[]";
            WebRequest request = WebRequest.Create(
                "https://personalpages.manchester.ac.uk/staff/grigory.pishchulov/Hierarchy.json");
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                n = reader.ReadToEnd();
            }
            App.nodes = JsonConvert.DeserializeObject<Node[]>(n);
        }

        protected override void OnSleep ()
        {
            string n = JsonConvert.SerializeObject(App.nodes);
            Application.Current.Properties["nodes"] = n;
        }

        protected override void OnResume ()
        {
            if (Application.Current.Properties.ContainsKey("nodes"))
            {
                string s = (string)Application.Current.Properties["nodes"];
                App.nodes = JsonConvert.DeserializeObject<Node[]>(s);
            }
        }
        public static Node FindNodeByName(string name)
        {
            return nodes.FirstOrDefault(node => node.Name == name);
        }
        public static List<Node> FindAlternatives()
        {
            List<Node> alternatives = new List<Node>();
            if (nodes != null)
            {
                foreach (Node node in nodes)
                {
                    if (node.Children == null || node.Children.Count == 0)
                    {
                        alternatives.Add(node);
                    }
                }
            }
            return alternatives;
        }
    }
    public class Node
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Children")]
        public List<string> Children { get; set; }

        public Dictionary<string, double> Priorities { get; set; } = new Dictionary<string, double>();

        public string DisplayPriority { get; set; }

        public double GlobalPriority { get; set; }

        public string DisplayGlobalPriority
        {
            get
            {
                return GlobalPriority == 0 ? "" : $"{GlobalPriority:P2}";
            }
        }
        public string FormattedGlobalPriority
        {
            get
            {
                return $"{GlobalPriority * 100:F2}%";
            }
        }
    }
    public class Preference
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public bool IsItem2Preferred { get; set; }
        public int Strength { get; set; }
        public double GetStrengthValue()
        {
            return IsItem2Preferred ? Strength : 1.0 / Strength;
        }
        public string StrengthDescription { get; set; }
    }
}