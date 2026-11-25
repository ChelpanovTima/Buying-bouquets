using Buying_bouquets.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Buying_bouquets
{
    public class CartRow
    {
        public int BouquetId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }

    public partial class CatalogWindow : Window
    {
        private readonly int userId;
        private List<CartRow> cartRows = new List<CartRow>();

        public CatalogWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            using var db = new FlowerShopContext();
            var user = db.Users.FirstOrDefault(u => u.Id == this.userId);
            UserInfoText.Text = user != null ? $"Корзина ({user.FullName}):" : "Корзина:";

            CatalogListBox.ItemsSource = db.Bouquets.OrderBy(b => b.Id).ToList();
            RefreshCart();
        }

        private void RefreshCart()
        {
            using var db = new FlowerShopContext();
            cartRows = db.CartItems
                .Include(c => c.Bouquet)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Id)
                .Select(c => new CartRow
                {
                    BouquetId = c.BouquetId,
                    Title = c.Bouquet.Title,
                    Price = c.Bouquet.Price,
                    Quantity = c.Quantity
                })
                .ToList();

            CartDataGrid.ItemsSource = cartRows;
            decimal total = cartRows.Sum(x => x.TotalPrice);
            TotalTextBlock.Text = $"Итого: {total:N2} руб.";
        }

        private void CatalogListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CatalogListBox.SelectedItem is Bouquet selected)
            {
                using var db = new FlowerShopContext();
                var item = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.BouquetId == selected.Id);
                if (item != null)
                {
                    item.Quantity += 1;
                }
                else
                {
                    db.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        BouquetId = selected.Id,
                        Quantity = 1
                    });
                }
                db.SaveChanges();
                RefreshCart();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CartDataGrid.SelectedItem is CartRow selected)
            {
                using var db = new FlowerShopContext();
                var item = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.BouquetId == selected.BouquetId);
                if (item != null)
                {
                    if (item.Quantity > 1)
                        item.Quantity -= 1;
                    else
                        db.CartItems.Remove(item);

                    db.SaveChanges();
                }
                RefreshCart();
            }
            else
            {
                MessageBox.Show("Выберите позицию в корзине.");
            }
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartRows.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }

            decimal total = cartRows.Sum(x => x.TotalPrice);
            CheckoutWindow checkout = new CheckoutWindow(userId, total);
            if (checkout.ShowDialog() == true)
            {
                RefreshCart();
            }
        }
    }
}