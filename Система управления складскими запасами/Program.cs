using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Система_управления_складскими_запасами
{
    interface ITransferable
    {
        void TransferTo(WareHouse targetWareHouse, int quantity);
    }

    abstract class WareHouseItem : ITransferable
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public decimal  Price { get; set; }

        public int Quantity { get; set; }

        public DateTime LastUpdate { get; set; }

        public abstract decimal CalculateStorageCost(int days);

        public void TransferTo(WareHouse targetWareHouse, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Колличество предметов должно быть больше нуля");
            }

            if (quantity > this.Quantity)
            {
                throw new InvalidOperationException($"Недостаточно товара на складе. Доступно: {this.Quantity}.");
            }

            this.Quantity -= quantity;

            var existingItem = targetWareHouse.Items.FirstOrDefault(item => item.ID == this.ID);

            if (existingItem is not null)
            {
                existingItem.Quantity += quantity;
                existingItem.LastUpdate = DateTime.Now;
            }
            else
            {
                WareHouseItem newItem = (WareHouseItem)this.MemberwiseClone();
                newItem.Quantity = quantity;
                newItem.LastUpdate = DateTime.Now;

                targetWareHouse.AddItem(newItem);
            }
        }

        protected WareHouseItem(int iD, string name, string category, decimal price, int quantity, DateTime lastUpdate)
        {
            ID = iD;

            Name = name;

            Category = category;

            Price = price;

            Quantity = quantity;

            LastUpdate = lastUpdate;
        }
    }

    class Electronics : WareHouseItem
    {

        public int WarrantyPeriod { get; set; }

        public override decimal CalculateStorageCost(int days)
        {
            return Price * 0.01m * days;
        }

        public Electronics(int iD, string name, string category, decimal price, int quantity, DateTime lastUpdate, int warrantyPeriod) 
            : base(iD, name, category, price, quantity, lastUpdate)
        {
            WarrantyPeriod = warrantyPeriod;
        }
    }

    class Food : WareHouseItem
    {
        public DateTime ExpirationDate { get; set; }

        public override decimal CalculateStorageCost(int days)
        {
            if(days < 30)
            {
                return Price * 0.005m * days * 2;
            }

            return Price * 0.005m * days;
        }

        public Food(int iD, string name, string category, decimal price, int quantity, DateTime lastUpdate, DateTime expirationDate) 
            : base(iD, name, category, price, quantity, lastUpdate)
        {
            ExpirationDate = expirationDate;
        }
    }

    class Furniture : WareHouseItem
    {
        public string Dimesions { get; set; }

        public override decimal CalculateStorageCost(int days)
        {
            return Price * 5 * days;
        }
        public Furniture(int iD, string name, string category, decimal price, int quantity, DateTime lastUpdate, string dimesions) 
            : base(iD, name, category, price, quantity, lastUpdate)
        {
            Dimesions = dimesions;
        }
    }

    class WareHouse
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public List<WareHouseItem> Items { get; private set; }

        public WareHouse(int id, string name, string address)
        {
            ID = id;

            Name = name;

            Address = address;

            Items = new List<WareHouseItem>();
        }

        public void AddItem(WareHouseItem item)
        {
            Items.Add(item);
        }

        public void RemoveItem(WareHouseItem item)
        {
            Items.Remove(item);
        }

        public decimal GetTotalValue()
        {
            decimal total = 0;

            foreach (WareHouseItem item in Items)
            {
                total += item.Price;
            }

            return total;
        }
         
        public void GetItemsByCategory (string category)
        {
            Console.WriteLine("Найден предмет: ");

            var query = Items.Where(item  => item.Category == category).ToList();

            foreach (WareHouseItem item in query)
            {
                Console.WriteLine(item);
            }
        }
    }

    public enum ItemType
    {
        Electronics,
        Food,
        Furniture
    }

    class WareHouseItemFactory
    {
        public WareHouseItem CreateItem(ItemType itemType, int id, string name, string category, decimal price, int quantity, DateTime lastUpdate, params object[] extraParams)
        {
            switch (itemType)
            {
                case ItemType.Electronics:
                    int warrantyPeriod = extraParams.Length > 0 ? (int)extraParams[0] : 0;
                    return new Electronics(id, name, category, price, quantity, lastUpdate, warrantyPeriod);

                case ItemType.Food:
                    DateTime expirationDate = extraParams.Length > 0 ? (DateTime)extraParams[0] : DateTime.Now.AddDays(7);
                    return new Food(id, name, category, price, quantity, lastUpdate, expirationDate);

                case ItemType.Furniture:
                    string dimession = extraParams.Length > 0 ? (string)extraParams[0] : "Не указаны";
                    return new Furniture(id, name, category, price, quantity, lastUpdate, dimession);

                default:
                    throw new ArgumentException("Указан неизвестный тип товара для создания");
            }
        }
    }

    class WareHouseFactory
    {
        public WareHouse CreateWareHouse(int id, string name, string address)
        {
            return new WareHouse(id, name, address);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            WareHouse mainWareHouse = new WareHouse(1, "Главный склад", "ул. Центральная д.1");

            WareHouse spareWareHouse = new WareHouse(2, "Запасной склад", "ул. Вторичная д.2");

            WareHouseFactory wareHouseFactory = new WareHouseFactory();

            WareHouseItemFactory itemFactory = new WareHouseItemFactory();

            mainWareHouse.AddItem(itemFactory.CreateItem(ItemType.Electronics, 2, "Ноутбук", "Компьютерная техника", 89000, 10, DateTime.Now, 3));
            mainWareHouse.AddItem(itemFactory.CreateItem(ItemType.Food, 3, "Рис", "Крупа", 120, 1000, DateTime.Now, 31));
            mainWareHouse.AddItem(itemFactory.CreateItem(ItemType.Furniture, 4, "Шкаф", "Мебель", 5000, 5, DateTime.Now, "120x100x200"));

            bool isRuning = true;

            while (isRuning)
            {
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║      СИСТЕМА УПРАВЛЕНИЯ СКЛАДАМИ       ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine("║ 1. Добавить товар на склад.            ║");
                Console.WriteLine("║ 2. Удалить товар со склада.            ║");
                Console.WriteLine("║ 3. Переместить товар между складами.   ║");
                Console.WriteLine("║ 4. Добавить склад.                     ║");
                Console.WriteLine("║ 5. Удалить склад.                      ║");
                Console.WriteLine("║ 6. Показать все склады.                ║");
                Console.WriteLine("║ 7. Показать все товары на складе.      ║");
                Console.WriteLine("║ 8. Показать общую стоимость склада.    ║");
                Console.WriteLine("║ 9. Найти товары по категории.          ║");
                Console.WriteLine("║ 10. Рассчитать стоимость хранения.     ║");
                Console.WriteLine("║ 11. Выход (сохранение данных).         ║");
                Console.WriteLine("╚════════════════════════════════════════╝");

                if (!int.TryParse(Console.ReadLine(), out int userChoice))
                {
                    Console.WriteLine("Некорректный ввод.");
                    continue;
                }

                try
                {
                    switch (userChoice)
                    {
                        case 1:
                            Console.WriteLine("Выбирете тип предмета:");
                            Console.WriteLine("╔════════════════════════════════════════╗");
                            Console.WriteLine("║ 1. Электроника                         ║");
                            Console.WriteLine("║ 2. Еда                                 ║");
                            Console.WriteLine("║ 3. Мебель                              ║");
                            Console.WriteLine("╚════════════════════════════════════════╝");

                            if (!int.TryParse(Console.ReadLine(), out int itemTypeChoice))
                            {
                                Console.WriteLine("Некорректный ввод.");
                                continue;
                            }

                            Console.WriteLine("Введите ID:");
                            int itemID = int.Parse(Console.ReadLine());

                            Console.WriteLine("Введите название:");
                            string itemName = Console.ReadLine();

                            Console.WriteLine("Введите категорию:");
                            string itemCategory = Console.ReadLine();

                            Console.WriteLine("Введите цену:");
                            decimal itemPrice = decimal.Parse(Console.ReadLine());

                            Console.WriteLine("Введите количество:");
                            int itemQuantity = int.Parse(Console.ReadLine());

                            DateTime itemLastUpdate = DateTime.Now;

                            try
                            {
                                switch(itemTypeChoice)
                                {
                                    case 1:
                                        Console.WriteLine("Введите гарантийный срок:");
                                        int electronicsWarrantyPeriod = int.Parse(Console.ReadLine());

                                        mainWareHouse.AddItem(itemFactory.CreateItem(ItemType.Electronics, itemID, itemName, itemCategory, itemPrice, itemQuantity, itemLastUpdate, electronicsWarrantyPeriod));

                                        Console.WriteLine("Товар добавлен.");
                                        break;

                                    case 2:
                                        DateTime foodExpirationDate;
                                        while (true)
                                        {
                                            Console.WriteLine("Введите дату окончания срока (в формате ДД.ММ.ГГГГ, например 15.08.2026):");
                                            if (DateTime.TryParse(Console.ReadLine(), out foodExpirationDate))
                                            {
                                                break; 
                                            }
                                            Console.WriteLine("Некорректный ввод.");
                                        }

                                        mainWareHouse.AddItem(itemFactory.CreateItem(ItemType.Food, itemID, itemName, itemCategory, itemPrice, itemQuantity, itemLastUpdate, foodExpirationDate));

                                        Console.WriteLine("Товар добавлен.");
                                        break;

                                    case 3:
                                        Console.WriteLine("Введите габариты: ");
                                        string furnitureDimession = Console.ReadLine();

                                        mainWareHouse.AddItem(itemFactory.CreateItem(ItemType.Furniture, itemID, itemName, itemCategory, itemPrice, itemQuantity, itemLastUpdate, furnitureDimession));

                                        Console.WriteLine("Товар добавлен.");
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка {ex.Message}.");
                            }

                            break;

                        case 2:
                            Console.WriteLine("Введите ID: ");

                            if(!int.TryParse(Console.ReadLine(), out int IDToRemove))
                            {
                                Console.WriteLine("Некорректный ID");
                                break;
                            }

                            WareHouseItem itemToRemove = mainWareHouse.Items.FirstOrDefault(item => item.ID == IDToRemove);

                            if(itemToRemove is not null)
                            {
                                mainWareHouse.RemoveItem(itemToRemove);
                            }
                            else
                            {
                                Console.WriteLine("Товар с таким ID не найден на складе.");
                            }
                            break;

                        case 3:
                            Console.WriteLine("Введите ID товара, который хотите переместить:");

                            if(!int.TryParse(Console.ReadLine(), out int IDToTransfer))
                            {
                                Console.WriteLine("Некорректный ID");
                                break;
                            }

                            WareHouseItem itemToTransfer = mainWareHouse.Items.FirstOrDefault(item => item.ID == IDToTransfer);

                            if(itemToTransfer is null)
                            {
                                Console.WriteLine("Товар с таким ID не найден на Главном складе.");
                                break;
                            }

                            Console.WriteLine($"Найден товар: {itemToTransfer.Name} | Доступно: {itemToTransfer.Quantity} шт.");

                            Console.WriteLine("Введите колличество товара для перемещения: ");

                            if(!int.TryParse(Console.ReadLine(), out int transferQuantity))
                            {
                                Console.WriteLine("Некорректный ввод количества.");
                                break;
                            }



                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка {ex.Message}");
                }
            }
        }
    }
}
