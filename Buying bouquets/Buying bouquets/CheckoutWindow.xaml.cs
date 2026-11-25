using Buying_bouquets.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Buying_bouquets
{
    public partial class CheckoutWindow : Window
    {
        private readonly int _userId;
        private readonly decimal _total;

        public CheckoutWindow(int userId, decimal total)
        {
            InitializeComponent();
            _userId = userId;
            _total = total;

            try
            {
                using var db = new FlowerShopContext();
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Id == _userId);
                if (user != null)
                {
                    FullNameBox.Text = user.FullName;
                    PhoneBox.Text = user.Phone;
                    AddressBox.Text = user.Address;

                    if (!Regex.IsMatch(user.Address, @",\s*кв\.\s*\d+$"))
                    {
                        NoApartmentCheck.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных покупателя: {ex.Message}");
            }

            SummaryText.Text = $"К оплате: {_total:N2} руб.";
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string name = FullNameBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string address = AddressBox.Text.Trim();
            bool noApartment = NoApartmentCheck.IsChecked == true;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите ФИО.");
                return;
            }

            if (!Regex.IsMatch(phone, @"^\+7 \d{3} \d{3}-\d{2}-\d{2}$"))
            {
                MessageBox.Show("Телефон должен соответствовать шаблону: +7 XXX XXX-XX-XX");
                return;
            }

            string addressPattern = noApartment
                ? @"^ул\.\s+[А-Яа-яЁё0-9\s\-]+,\s*д\.\s*[0-9А-Яа-я/]+$"
                : @"^ул\.\s+[А-Яа-яЁё0-9\s\-]+,\s*д\.\s*[0-9А-Яа-я/]+,\s*кв\.\s*\d+$";

            if (!Regex.IsMatch(address, addressPattern))
            {
                MessageBox.Show(noApartment
                    ? "Неверный адрес. Шаблон: ул. [улица], д. [дом]"
                    : "Неверный адрес. Шаблон: ул. [улица], д. [дом], кв. [квартира]");
                return;
            }

            try
            {
                using var db = new FlowerShopContext();
                var cartItems = db.CartItems
                    .Where(c => c.UserId == _userId)
                    .Join(db.Bouquets, c => c.BouquetId, b => b.Id, (c, b) => new { c, b })
                    .ToList();

                if (cartItems.Count == 0)
                {
                    MessageBox.Show("Корзина пуста.");
                    return;
                }

                decimal actualTotal = cartItems.Sum(x => x.b.Price * x.c.Quantity);

                var order = new Order
                {
                    UserId = _userId,
                    RecipientName = name,
                    RecipientPhone = phone,
                    DeliveryAddress = address,
                    TotalAmount = actualTotal,
                    CreatedAt = DateTime.Now
                };

                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var row in cartItems)
                {
                    db.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        BouquetId = row.b.Id,
                        Quantity = row.c.Quantity,
                        PriceAtOrder = row.b.Price
                    });
                }

                db.CartItems.RemoveRange(cartItems.Select(x => x.c));
                db.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен!");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }
    }
}