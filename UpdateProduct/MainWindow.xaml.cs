using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
using WooCommerce.NET.WordPress.v2;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

namespace UpdateProduct
{

    public class sapoProduct
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public string barcode { get; set; }
    }

    public class orderProduct
    {
        public string Name { get; set; }

        public string SapoName { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; }
        public double Price { get; set; }
        public BitmapSource ProductImage { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WCObject wc = null;

        List<ProductCategory> categories = new List<ProductCategory>();
        public MainWindow()
        {
            InitializeComponent();

            RestAPI rest = new RestAPI("https://pisolution.tech/wp-json/wc/v3/", "ck_8707c0420f37da8d261035a42adf00d8e28ee932", "cs_a5b615a4c1b97d6da5bea550429c2aa1e8b5a22a");
            wc = new WCObject(rest);

            //loadWpCategories();

            sapo_source.SelectedCellsChanged += Sapo_source_SelectedCellsChanged;
        }

        void loadWpCategories()
        {
            categories = wc.Category.GetAll().Result;
        }

        void loadSapoPorducts()
        {
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open("product_03.01.2024.xlsx", false))
            {
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                if (sheet != null)
                {
                    WorkbookPart wbPart = doc.WorkbookPart;
                    Worksheet Worksheet = ((WorksheetPart)wbPart.GetPartById(sheet.Id)).Worksheet;

                    SheetData sheetData = Worksheet.GetFirstChild<SheetData>();

                    IEnumerable<Row> rows = sheetData.Descendants<Row>();

                    foreach (var row in rows)
                    {
                        for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            string val = GetCellValue(doc, row.Descendants<Cell>().ElementAt(i));
                        }
                    }
                }
            }
        }

        public string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell == null)
                return null;

            string value = cell.InnerText;
           
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        //Update to view
        private void Sapo_source_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Order order = sapo_source.SelectedItem as Order;
            if (order != null)
            {
                txtAddress.Text = order.billing.address_1;
                txtPhone.Text = order.billing.phone;
                txtpayment.Text = order.payment_method_title;

                List<orderProduct> orderProducts = new List<orderProduct>();
                foreach (var item in order.line_items)
                {
                    var product = wc.Product.Get((ulong)item.product_id).Result;

                    var atr = product.meta_data.FirstOrDefault(x => x.key == "sapo_name");


                    string sapoName = string.Empty;

                    if (atr != null)
                        sapoName = atr.value.ToString();

                    BitmapImage bitmap = new BitmapImage();

                    if(product.images.Count > 0)
                    {
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(product.images[0].src, UriKind.Absolute);
                        bitmap.EndInit();
                    }    
                    

                    orderProducts.Add(new orderProduct()
                    {
                        Name = item.name,
                        Quantity = (int)item.quantity,
                        Total = (double)item.total,
                        Price = (double)item.price,
                        ProductImage = bitmap,
                        SapoName = sapoName
                    });
                }
                order_products_lst.ItemsSource = orderProducts;
            }
        }

        private void updateCategories()
        {
            var categories = wc.Category.GetAll().Result;

            ListCollectionView lcv = new ListCollectionView(categories);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("parent"));
            sapo_source.ItemsSource = lcv;
        }


       

        private void refreshProduct()
        {
            var products = wc.Product.GetAll().Result;
            web_source.ItemsSource = products;
        }


        //Refresh product
        private  void Refresh_Click(object sender, RoutedEventArgs e)
        {
            refreshProduct();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Product p = new Product()
            {
                name = "test product 11",
                short_description = "test product 11",
                description = "miêu tả chi tiết hơn test product 11",
                regular_price = 80000,
                price =8000,
                sale_price = 120000,
                on_sale = true,
                stock_quantity = 10
            };
            await wc.Product.Add(p);
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {

            Product p = web_source.SelectedItem as Product;
            if(p != null)
            {
                if (p.price == null)
                    p.price = 0;

                decimal price = (decimal)(p.price + 10000);
                decimal salePrice = (decimal)(price + 10000);

                List<ProductCategoryLine> appliedCategories = new List<ProductCategoryLine>();

                appliedCategories.Add(new ProductCategoryLine()
                {
                    id = this.categories[0].id,
                    name = this.categories[0].name
                });

                await wc.Product.Update((ulong)p.id, new Product { price = price,  regular_price = price, sale_price = salePrice, categories = appliedCategories });
                

            }    
            
        }

        private void Button_OrderUpdate(object sender, RoutedEventArgs e)
        {
            Order order = sapo_source.SelectedItem as Order;
            if (order != null)
            {
                string status = "processing";
                if (order.status == "processing")
                    status = "on-hold";
                else if (order.status == "on-hold")
                    status = "completed";

                wc.Order.Update((ulong)order.id, new Order
                {
                    status = status
                });
            }
        }


        private void Button_Orders(object sender, RoutedEventArgs e)
        {
            
            var products = wc.Order.GetAll().Result;
            sapo_source.ItemsSource = products;
            
        }
    }
}
