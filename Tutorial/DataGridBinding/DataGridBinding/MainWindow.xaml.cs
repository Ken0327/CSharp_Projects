using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace DataGridBinding
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            QueryGPNList();
        }

        List<GPNList> gpnList_Original = new List<GPNList>();
        List<GPNList> gpnList_Modified = new List<GPNList>();
        GPNList gpn_Modified = new GPNList();
        string GPNListPath = "C:\\LaserMaster\\GPNList.xml";

        private void QueryGPNList()
        {
            gpnList_Original.Clear();
            gpnList_Modified.Clear();
            var allGPN = XDocument.Load(GPNListPath).Element("GPNList").Elements("GPN");
            foreach (var GPN in allGPN)
            {
                var gpn = GPN.Attribute("num").Value;
                var product = GPN.Attribute("ProductCate").Value;
                gpnList_Original.Add(new GPNList()
                {
                    GPN = gpn,
                    Product = product
                });
                gpnList_Modified.Add(new GPNList()
                {
                    GPN = gpn,
                    Product = product
                });
            }
            datagrid_gpnlist.ItemsSource = gpnList_Modified;

            //DataSet dataSet = new DataSet();
            //dataSet.ReadXml("C:\\LaserMaster\\GPNList.xml");
            //datagrid_gpnlist.ItemsSource = dataSet.Tables[0].DefaultView;
        }

        public class GPNList
        {
            public string GPN { get; set; }
            public string Product { get; set; }
        }

        private void gpnlist_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                gpn_Modified.GPN = (datagrid_gpnlist.Items[datagrid_gpnlist.SelectedIndex] as GPNList).GPN;
                gpn_Modified.Product = (datagrid_gpnlist.Items[datagrid_gpnlist.SelectedIndex] as GPNList).Product;
                //MessageBox.Show("你已選擇 \n料號: " + gpn_Modified.GPN + "\n產品: " + gpn_Modified.Product);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SearchGPN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InsertGPN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteGPN_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_gpnlist.SelectedItem != null)
            {
                datagrid_gpnlist.Items.Remove((GPNList)datagrid_gpnlist.SelectedItem);
            }
        }

        private void ModifyGPN_Click(object sender, RoutedEventArgs e)
        {
            gpnList_Modified = datagrid_gpnlist.ItemsSource as List<GPNList>;
            for (var i = 0; i < gpnList_Modified.Count; i++)
            {
                if (gpnList_Modified[i].GPN != gpnList_Original[i].GPN || gpnList_Modified[i].Product != gpnList_Original[i].Product)
                {
                    System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(
                        "請問是否修改為\n" + "GPN:\n" + gpnList_Original[i].GPN + " -> " + gpnList_Modified[i].GPN + "\n" + "ProductCate:\n" + gpnList_Original[i].Product + " -> " + gpnList_Modified[i].Product,
                        "Modify GPN remindation?", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Information);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        // write to GPN List xml
                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.OmitXmlDeclaration = true;

                        XmlWriter xmlWriter = XmlWriter.Create(GPNListPath, settings);
                        xmlWriter.WriteStartDocument(true);
                        // Write first element
                        xmlWriter.WriteStartElement("GPNList");
                        foreach (var item in gpnList_Modified)
                        {
                            xmlWriter.WriteStartElement("GPN");
                            xmlWriter.WriteAttributeString("num", item.GPN);
                            xmlWriter.WriteAttributeString("ProductCate", item.Product);
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndDocument();
                        xmlWriter.Close();

                        MessageBox.Show("已修改為\n" + "GPN: " + gpnList_Modified[i].GPN + "\n" + "ProductCate: " + gpnList_Modified[i].Product);
                        
                        // update datagrid
                        QueryGPNList();
                    }
                }
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Search_Text_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
