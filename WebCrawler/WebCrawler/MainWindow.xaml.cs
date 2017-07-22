using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using HtmlAgilityPack;

namespace WebCrawler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static SqlConnection c = new SqlConnection("Data Source=Hamza;Initial Catalog=Webs;Integrated Security=True;Pooling=False");
        WebClient client = new WebClient();
        int ID;
        int limit;
        public struct LinkItem
        {
            public string Href;
            public string Text;
        }
        static void Query(string Query)
        {
            c.Open();
            try
            {
                new SqlCommand(Query, c).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                c.Close();
                throw ex;
            }
            c.Close();
        }
        
        public MainWindow()
        {
            InitializeComponent();
            c.Open();
            try
            {
                ID=int.Parse(new SqlCommand("Select MAX(ID) from WEB", c).ExecuteScalar().ToString());
            }
            catch
            {
                ID = 0;
                c.Close();
            }
            c.Close();
        }

        static class LinkFinder
        {
            public static List<LinkItem> Find(string file)
            {
                List<LinkItem> list = new List<LinkItem>();

                // 1.
                // Find all matches in file.
                MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
                    RegexOptions.Singleline);

                // 2.
                // Loop over each match.
                foreach (Match m in m1)
                {
                    string value = m.Groups[1].Value;
                    LinkItem i = new LinkItem();

                    // 3.
                    // Get href attribute.
                    Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                        RegexOptions.Singleline);
                    if (m2.Success)
                    {
                        i.Href = m2.Groups[1].Value;
                    }

                    // 4.
                    // Remove inner tags from text.
                    string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                        RegexOptions.Singleline);
                    i.Text = t;

                    list.Add(i);
                }
                return list;
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (URL.Text == "" || !Uri.IsWellFormedUriString(URL.Text, UriKind.Absolute))
            {
                MessageBox.Show("Invalid URL");
            }
            else
            {
                limit = Limit.Text == "" ? 100 : int.Parse(Limit.Text);
                    Crawler(URL.Text, Depth.Text == "" ? 1 : int.Parse(Depth.Text));
                    MessageBox.Show("Processing Completed");
            }
            
        }
        public static string ExtractText(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var chunks = new List<string>();

            foreach (var item in doc.DocumentNode.DescendantsAndSelf())
            {
                if (item.NodeType == HtmlNodeType.Text)
                {
                    if (item.InnerText.Trim() != "")
                    {
                        chunks.Add(item.InnerText.Trim());
                    }
                }
            }
            return String.Join(" ", chunks);
        }

        void Crawler(string URL, int Depth)
        {
            ProgressBar.Value = limit;
            if (limit!=0)
            {
                string htmlCode=null;
                try
                {
                    htmlCode = client.DownloadString(URL);
                }
                catch
                {
                    MessageBox.Show("Internet Connection is not working.");
                }

                string title = Regex.Match(htmlCode, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;

                try
                {
                    ID++;
                    string Page= ExtractText(htmlCode).Replace("'", "''");
                    System.IO.File.WriteAllText($"{ID}.html", htmlCode);
                    Query($"Insert into web values ('{URL}','{ID}','{title.Substring(0, title.Length > 98 ? 98 : title.Length)}','{Page.Substring(0, Page.Length> 7999?7999:Page.Length-1)}');");
                    limit--;
                }
                catch (Exception ex) { ID--; }// MessageBox.Show(ex.Message); }

                Depth--;
                foreach (LinkItem i in LinkFinder.Find(htmlCode))
                {
                    try
                    {
                        if (Depth > 0)
                            if (Uri.IsWellFormedUriString(i.Href, UriKind.Absolute))
                                Crawler(i.Href, Depth);
                    }
                    catch
                    { }
                }
            }
        }

        private void NumericValidation(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back && !(e.Key >= Key.D0 && e.Key <= Key.D9))
                e.Handled = true;
        }
    }
}
