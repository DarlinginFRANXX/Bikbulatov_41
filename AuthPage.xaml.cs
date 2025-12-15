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
using System.Windows.Threading;

namespace Bikbulatov41
{
    public partial class AuthPage : Page
    {
        private string currentCaptcha;
        private DispatcherTimer blockTimer;
        private int blockTimeRemaining = 0;
        private int failedAttempts = 0;

        public AuthPage()
        {
            try
            {
                InitializeComponent();
                InitializeTimer();
                // Изначально капча скрыта
                HideCaptcha();
                // Убедимся что счетчик ошибок сброшен
                failedAttempts = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HideCaptcha()
        {
            try
            {
                CaptchaLabel.Visibility = Visibility.Collapsed;
                CaptchaPanel.Visibility = Visibility.Collapsed;
                CaptchaInputLabel.Visibility = Visibility.Collapsed;
                CaptchaInputBox.Visibility = Visibility.Collapsed;
                CaptchaInputBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скрытии капчи: {ex.Message}", "Ошибка");
            }
        }

        private void ShowCaptcha()
        {
            try
            {
                CaptchaLabel.Visibility = Visibility.Visible;
                CaptchaPanel.Visibility = Visibility.Visible;
                CaptchaInputLabel.Visibility = Visibility.Visible;
                CaptchaInputBox.Visibility = Visibility.Visible;
                GenerateHardCaptcha();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при показе капчи: {ex.Message}", "Ошибка");
            }
        }

        private void InitializeTimer()
        {
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer_Tick;
        }

        private void GenerateHardCaptcha()
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new Random(Guid.NewGuid().GetHashCode());
                StringBuilder captchaBuilder = new StringBuilder();

                for (int i = 0; i < 6; i++)
                {
                    captchaBuilder.Append(chars[random.Next(chars.Length)]);
                }

                currentCaptcha = captchaBuilder.ToString();

                // Очищаем панель капчи
                CaptchaPanel.Children.Clear();

                // Создаем контейнер для капчи с помехами
                var border = new Border();
                border.BorderBrush = Brushes.Black;
                border.BorderThickness = new Thickness(1);
                border.Background = Brushes.LightYellow;
                border.Padding = new Thickness(10);
                border.Width = 150;
                border.Height = 50;

                // Canvas для рисования помех
                var canvas = new Canvas();
                canvas.Width = 150;
                canvas.Height = 50;

                // Добавляем случайные линии-помехи
                for (int i = 0; i < 8; i++)
                {
                    var line = new Line();
                    line.X1 = random.Next(0, 150);
                    line.Y1 = random.Next(0, 50);
                    line.X2 = random.Next(0, 150);
                    line.Y2 = random.Next(0, 50);
                    line.Stroke = new SolidColorBrush(
                        Color.FromArgb((byte)random.Next(100, 150),
                                     (byte)random.Next(100, 200),
                                     (byte)random.Next(100, 200),
                                     (byte)random.Next(100, 200)));
                    line.StrokeThickness = random.Next(1, 2);
                    canvas.Children.Add(line);
                }

                // Добавляем текст капчи
                var textPanel = new StackPanel();
                textPanel.Orientation = Orientation.Horizontal;
                textPanel.HorizontalAlignment = HorizontalAlignment.Center;
                textPanel.VerticalAlignment = VerticalAlignment.Center;

                foreach (char c in currentCaptcha)
                {
                    var charBlock = new TextBlock();
                    charBlock.Text = c.ToString();
                    charBlock.FontSize = 20 + random.Next(-3, 4);
                    charBlock.FontWeight = FontWeights.Bold;
                    charBlock.Foreground = new SolidColorBrush(
                        Color.FromRgb((byte)random.Next(50, 150),
                                    (byte)random.Next(50, 150),
                                    (byte)random.Next(50, 150)));
                    charBlock.FontFamily = new FontFamily("Courier New");

                    // Небольшой наклон для каждого символа
                    charBlock.RenderTransform = new RotateTransform(random.Next(-5, 6));
                    charBlock.Margin = new Thickness(1, 0, 1, 0);

                    textPanel.Children.Add(charBlock);
                }

                // Собираем все вместе
                var grid = new Grid();
                grid.Children.Add(canvas);
                grid.Children.Add(textPanel);

                border.Child = grid;

                // Добавляем в основную панель
                CaptchaPanel.Children.Add(border);

                // Обязательно добавляем кнопку обновления обратно
                if (!CaptchaPanel.Children.Contains(RefreshCaptchaButton))
                {
                    RefreshCaptchaButton.Margin = new Thickness(10, 0, 0, 0);
                    CaptchaPanel.Children.Add(RefreshCaptchaButton);
                }

                // Очищаем поле ввода
                CaptchaInputBox.Text = "";
                CaptchaInputBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Упрощенная капча на случай ошибки
                MessageBox.Show($"Ошибка генерации капчи: {ex.Message}. Используется упрощенная версия.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Простая капча как fallback
                var random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                StringBuilder captchaBuilder = new StringBuilder();

                for (int i = 0; i < 6; i++)
                {
                    captchaBuilder.Append(chars[random.Next(chars.Length)]);
                }

                currentCaptcha = captchaBuilder.ToString();
                CaptchaTextBlock.Text = currentCaptcha;
                CaptchaInputBox.Text = "";
            }
        }

        private bool ValidateCaptcha()
        {
            try
            {
                string userInput = CaptchaInputBox.Text.Trim();
                return string.Equals(userInput, currentCaptcha, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void BlockLoginButton(int seconds)
        {
            try
            {
                LoginButton.IsEnabled = false;
                blockTimeRemaining = seconds;
                UpdateLoginButtonText();
                blockTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при блокировке кнопки: {ex.Message}", "Ошибка");
            }
        }

        private void UnblockLoginButton()
        {
            try
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
                blockTimer.Stop();
                blockTimeRemaining = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при разблокировке кнопки: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateLoginButtonText()
        {
            if (blockTimeRemaining > 0)
            {
                LoginButton.Content = $"Заблокировано ({blockTimeRemaining} сек)";
            }
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                blockTimeRemaining--;

                if (blockTimeRemaining <= 0)
                {
                    UnblockLoginButton();
                }
                else
                {
                    UpdateLoginButtonText();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в таймере: {ex.Message}", "Ошибка");
                blockTimer.Stop();
            }
        }

        private void LoginHowGuestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Manager.MainFrame.Navigate(new ProductPage(null));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе как гость: {ex.Message}", "Ошибка");
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!LoginButton.IsEnabled)
                {
                    MessageBox.Show($"Попробуйте снова через {blockTimeRemaining} секунд", "Кнопка заблокирована");
                    return;
                }

                string login = LoginBox.Text.Trim();
                string password = PasswordBox.Text.Trim();

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Заполните логин и пароль", "Ошибка");
                    return;
                }

                // Проверяем, нужно ли показывать/проверять капчу
                bool captchaRequired = CaptchaInputBox.Visibility == Visibility.Visible;

                if (captchaRequired)
                {
                    string captchaInput = CaptchaInputBox.Text.Trim();

                    if (string.IsNullOrWhiteSpace(captchaInput))
                    {
                        MessageBox.Show("Введите капчу", "Ошибка");
                        return;
                    }

                    if (!ValidateCaptcha())
                    {
                        MessageBox.Show("Неверная капча! Попробуйте снова.", "Ошибка");
                        GenerateHardCaptcha();
                        BlockLoginButton(10); // Блокировка только при неправильной капче
                        return;
                    }
                }

                // Пытаемся найти пользователя в базе данных
                User user = null;
                try
                {
                    var context = Bikbulatov41Entities.GetContext();
                    user = context.User
                        .FirstOrDefault(p => p.UserLogin == login && p.UserPassword == password);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обращении к базе данных: {ex.Message}", "Ошибка базы данных");
                    return;
                }

                if (user != null)
                {
                    // Успешный вход
                    failedAttempts = 0;
                    HideCaptcha(); // Скрываем капчу после успешного входа
                    Manager.MainFrame.Navigate(new ProductPage(user));
                    LoginBox.Text = "";
                    PasswordBox.Text = "";
                }
                else
                {
                    failedAttempts++;
                    MessageBox.Show("Неверный логин или пароль", "Ошибка");

                    // Показываем капчу только после первой неудачной попытки
                    if (failedAttempts >= 1 && CaptchaInputBox.Visibility != Visibility.Visible)
                    {
                        ShowCaptcha();
                        // НЕ блокируем кнопку при первой ошибке, только показываем капчу
                    }
                    else if (failedAttempts >= 2 && CaptchaInputBox.Visibility == Visibility.Visible)
                    {
                        // Блокируем кнопку только после второй ошибки (когда уже есть капча)
                        GenerateHardCaptcha();
                        BlockLoginButton(10);
                    }
                    else
                    {
                        // Просто показываем сообщение об ошибке, без блокировки
                        MessageBox.Show("Неверный логин или пароль", "Ошибка");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCaptchaButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                GenerateHardCaptcha();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении капчи: {ex.Message}", "Ошибка");
            }
        }
    }
}