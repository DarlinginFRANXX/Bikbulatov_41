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

namespace Bikbulatov41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private User _currentUser;
        private List<Product> selectedProducts = new List<Product>();
        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        private int newOrderID = -1; // Временный ID
        // Конструктор для гостя
        private void InitializePage(User user)
        {
            if (user != null)
            {
                UserNameTextBlock.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                switch (user.UserRole)
                {
                    case 1:
                        UserRoleTextBlock.Text = "Клиент"; break;
                    case 2:
                        UserRoleTextBlock.Text = "Менеджер"; break;
                    case 3:
                        UserRoleTextBlock.Text = "Администратор"; break;
                }
                var currentProduct = Bikbulatov41Entities.GetContext().Product.ToList();
                ProductListView.ItemsSource = currentProduct;

                ComboType.SelectedIndex = 0;
                UpdateProduct();
            }
            else
            {
                UserNameTextBlock.Text = "Гость";
                UserRoleTextBlock.Text = "нету";
                var currentProduct = Bikbulatov41Entities.GetContext().Product.ToList();
                ProductListView.ItemsSource = currentProduct;

                ComboType.SelectedIndex = 0;
                UpdateProduct();

            }
        }
        public ProductPage(User user)
        {
            InitializeComponent();


            InitializePage(user);
            _currentUser = user;
        }

        // Общий метод загрузки продуктов



        private void UpdateProduct()
        {
            var allProducts = Bikbulatov41Entities.GetContext().Product.ToList();
            var currentProduct = allProducts.ToList();

            // ------ ФИЛЬТРАЦИЯ ------
            if (ComboType.SelectedIndex == 0)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 && Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
            }
            else if (ComboType.SelectedIndex == 1)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 && Convert.ToDouble(p.ProductDiscountAmount) < 9.99)).ToList();
            }
            else if (ComboType.SelectedIndex == 2)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 10 && Convert.ToDouble(p.ProductDiscountAmount) < 14.99)).ToList();
            }
            else if (ComboType.SelectedIndex == 3)
            {
                currentProduct = currentProduct.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 15 && Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
            }

            // ------ ПОИСК ------
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                currentProduct = currentProduct
                    .Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower()))
                    .ToList();
            }

            // ------ СОРТИРОВКА ------
            if (RButtonDown.IsChecked == true)
                currentProduct = currentProduct.OrderByDescending(p => p.ProductCost).ToList();
            else if (RButtonUp.IsChecked == true)
                currentProduct = currentProduct.OrderBy(p => p.ProductCost).ToList();

            // ------ ОБНОВЛЕНИЕ СПИСКА ------
            ProductListView.ItemsSource = currentProduct;

            // ------ ОБНОВЛЕНИЕ ТЕКСТА "Х из Y" ------
            TextCount.Text = $"{currentProduct.Count} из {allProducts.Count}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProduct();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void ShoeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedIndex >= 0)
            {
                var prod = ProductListView.SelectedItem as Product;
                selectedProducts.Add(prod);

                var newOrderProd = new OrderProduct(); // новый заказ
                newOrderProd.OrderID = newOrderID;

                // номер продукта в новую запись
                newOrderProd.ProductArticleNumber = prod.ProductArticleNumber;
                newOrderProd.Count = 1;

                // Проверяем, есть ли уже такой товар в заказе
                var selOP = selectedOrderProducts.Where(p =>
                    p.ProductArticleNumber == prod.ProductArticleNumber);

                if (selOP.Count() == 0)
                {
                    selectedOrderProducts.Add(newOrderProd);
                }
                else
                {
                    foreach (OrderProduct p in selectedOrderProducts)
                    {
                        if (p.ProductArticleNumber == prod.ProductArticleNumber)
                            p.Count++;
                    }
                }

                ViewOrderBtn.Visibility = Visibility.Visible;
                ProductListView.SelectedIndex = -1;
            }
        }
        public void ResetOrder()
        {
            // Очищаем списки заказа
            if (selectedOrderProducts != null)
                selectedOrderProducts.Clear();

            if (selectedProducts != null)
                selectedProducts.Clear();

            // Сбрасываем временный ID
            newOrderID = -1;

            // Скрываем кнопку "Посмотреть заказ"
            ViewOrderBtn.Visibility = Visibility.Collapsed;

        }

        private void ViewOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Убираем дубликаты, если они есть
                selectedProducts = selectedProducts.Distinct().ToList();

                // Проверяем, есть ли товары в заказе
                if (selectedProducts == null || selectedProducts.Count == 0)
                {
                    MessageBox.Show("Добавьте товары в заказ!", "Пустой заказ",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Передаем пользователя, если авторизован
                OrderWindow orderWindow = new OrderWindow(
                    selectedOrderProducts,
                    selectedProducts,
                    _currentUser);

                // Открываем окно заказа как диалог и получаем результат
                bool? result = orderWindow.ShowDialog();

                // Если заказ успешно сохранен (окно закрыто с true)
                if (result == true)
                {
                    // Сбрасываем состояние заказа
                    ResetOrder();
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
