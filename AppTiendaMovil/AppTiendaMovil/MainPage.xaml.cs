using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace AppTiendaMovil
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public MainPage()
        {
            InitializeComponent();

            // sample product
            Products.Add(new Product
            {
                Image = "dotnet_bot.png",
                Name = "GForce RTX5090 Asus Gaming",
                Brand = "Asus",
                Price = 79000.00m,
                Stock = 8,
                IsActive = true
            });

            BindingContext = this;
        }

        private void OnSearchClicked(object? sender, EventArgs e)
        {
            // For now this is a placeholder: real search should filter Products collection or query a service
            DisplayAlert("Buscar", $"Buscar: {SearchEntry.Text} - Incluir inactivos: {IncludeInactiveCheck.IsChecked}", "OK");
        }

        private void OnNewProductClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Nuevo producto", "Navegar a la pantalla de creación (no implementado)", "OK");
        }

        private void OnEditClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Editar", "Editar producto (no implementado)", "OK");
        }

        private void OnImageClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Imagen", "Administrar imagen (no implementado)", "OK");
        }

        private async void OnDeleteClicked(object? sender, EventArgs e)
        {
            var ok = await DisplayAlert("Eliminar", "¿Desea eliminar este producto?", "Sí", "No");
            if (ok)
            {
                // find product from sender context
                if (sender is VisualElement ve && ve.BindingContext is Product p)
                {
                    Products.Remove(p);
                }
            }
        }

    public class Product
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }

        public string StatusText => IsActive ? "Activo" : "Inactivo";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Gray;
    }
          
       
    }
}
