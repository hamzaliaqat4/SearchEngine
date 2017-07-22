using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace Search_Engine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        SqlConnection c = new SqlConnection("Data Source=Hamza;Initial Catalog=Webs;Integrated Security=True;Pooling=False");
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private void Search(object sender, RoutedEventArgs e)
        {
            c.Open();
            if (TB.Text == "")
                MessageBox.Show("Please enter the text to be searched.");
            else
                try
                {
                    DataTable t = new DataTable();

                    string[] words = TB.Text.Split(' ');
                    string s = null;
                    string des = "case ";
                    string order = "Order By (";
                    foreach (string word in words)
                    {
                        des += $" when Page like '% {word} %' then SUBSTRING(Page,CHARINDEX(' {word} ',Page)-50,100)";
                        s += $"title like '% {word} %' OR page like '% {word} %' OR ";
                        order += $" Case When page like '% {word} %' Then 1 Else 0 End + Case When title like '% {word} %' Then 10 Else 0 End +";
                    }
                    order = order.Remove(order.Length - 1) + " ) Desc";
                    des += " else '50' end";

                    SqlCommand a = new SqlCommand($"select URL,ID,Title,{des} as Description from web where {s.Remove(s.Length - 3)} {order}", c);

                    SqlDataAdapter d = new SqlDataAdapter(a);
                    d.Fill(t);
                    lvDataBinding.ItemsSource = t.DefaultView;
                    G.ItemsSource = t.DefaultView;
                    G.CanUserAddRows = false;
                    if (t.Rows.Count == 0) MessageBox.Show("No Result Found!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            c.Close();

        }

        private void LinkClicked(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Do you want to open it online? ", "Opening Page", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start((sender as TextBox).Text);
            }
            else
            {
                c.Open();
                string id = new SqlCommand($"Select ID from web where URL = '{(sender as TextBox).Text}'",c).ExecuteScalar().ToString();
                System.Diagnostics.Process.Start(id+".html");
                c.Close();

            }



        }
    }
}
