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

            categories = wc.Category.GetAll().Result;
        }

        private void updateCategories()
        {
            var categories = wc.Category.GetAll().Result;

            ListCollectionView lcv = new ListCollectionView(categories);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("parent"));
            sapo_source.ItemsSource = lcv;
        }

        private void refreshOrders()
        {
            var products = wc.Order.GetAll().Result;
            sapo_source.ItemsSource = products;
        }

        private void refreshProduct()
        {
            var products = wc.Product.GetAll().Result;
            web_source.ItemsSource = products;
        }

        private  void Refresh_Click(object sender, RoutedEventArgs e)
        {
            //refreshOrders();
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
    }
}
