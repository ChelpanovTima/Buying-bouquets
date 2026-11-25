using Buying_bouquets.Models;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Buying_bouquets
{
    public partial class MainWindow : Window
    {
        private bool isRegisterMode = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SwitchModeButton_Click(object sender, RoutedEventArgs e)
        {
            isRegisterMode = !isRegisterMode;

            if (isRegisterMode)
            {
                Title = "Регистрация";
                HeaderText.Text = "Регистрация пользователя";
                RegisterPanel.Visibility = Visibility.Visible;
                ActionButton.Content = "Зарегистрироваться";
                SwitchModeButton.Content = "Войти";
            }
            else
            {
                Title = "Авторизация";
                HeaderText.Text = "Вход в систему";
                RegisterPanel.Visibility = Visibility.Collapsed;
                ActionButton.Content = "Войти";
                SwitchModeButton.Content = "Зарегистрироваться";
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            using var db = new FlowerShopContext();

            if (!isRegisterMode)
            {
                User? user = db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
                if (user != null)
                {
                    CatalogWindow catalog = new CatalogWindow(user.Id);
                    catalog.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
            else
            {
                string fullName = FullNameBox.Text.Trim();
                string phone = PhoneBox.Text.Trim();
                string address = AddressBox.Text.Trim();
                bool noApartment = NoApartmentCheck.IsChecked == true;

                if (string.IsNullOrEmpty(fullName))
                {
                    MessageBox.Show("Введите ФИО.");
                    return;
                }

                if (!Regex.IsMatch(phone, @"^\+7 \d{3} \d{3}-\d{2}-\d{2}$"))
                {
                    MessageBox.Show("Телефон должен быть в формате: +7 XXX XXX-XX-XX");
                    return;
                }

                string addressPattern = noApartment ? @"^ул\.\s+[А-Яа-яЁё0-9\s\-]+,\s*д\.\s*[0-9А-Яа-я/]+$"
                    : @"^ул\.\s+[А-Яа-яЁё0-9\s\-]+,\s*д\.\s*[0-9А-Яа-я/]+,\s*кв\.\s*\d+$";

                if (!Regex.IsMatch(address, addressPattern))
                {
                    MessageBox.Show(noApartment ? "Формат адреса: ул. [улица], д. [дом]"
                        : "Формат адреса: ул. [улица], д. [дом], кв. [квартира]");
                    return;
                }

                
                if (db.Users.Any(u => u.Login == login))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                    return;
                }

                var newUser = new User
                {
                    Login = login,
                    Password = password,
                    FullName = fullName,
                    Phone = phone,
                    Address = address
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация успешна!");
                SwitchModeButton_Click(null, null);
            }
        }
    }
}