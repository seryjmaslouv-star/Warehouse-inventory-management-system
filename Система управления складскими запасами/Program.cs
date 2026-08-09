using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Система_управления_складскими_запасами;

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
                total += item.Price * item.Quantity;
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
            List<WareHouse> wareHouses = new List<WareHouse>();

            WareHouseFactory wareHouseFactory = new WareHouseFactory();

            WareHouseItemFactory itemFactory = new WareHouseItemFactory();

            wareHouses.Add(wareHouseFactory.CreateWareHouse(1, "Главный склад", "ул. Центральная д.1"));

            wareHouses.Add(wareHouseFactory.CreateWareHouse(2, "Запасной склад", "ул. Вторичная д.2"));

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
                            AddItemToWH(wareHouses, itemFactory);
                            break;

                        case 2:
                            Console.WriteLine("Введите ID склада, с которого будет удалён товар: ");
                            if(!int.TryParse(Console.ReadLine(), out int whIDToRemove) || wareHouses.FirstOrDefault(wh => wh.ID == whIDToRemove) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            WareHouse targetWhToRemoveItem = wareHouses.First(wh  => wh.ID == whIDToRemove);

                            Console.WriteLine("Введите ID: ");
                            if (!int.TryParse(Console.ReadLine(), out int IDToRemove))
                            {
                                Console.WriteLine("Некорректный ID");
                                break;
                            }

                            var itemToRemove = targetWhToRemoveItem.Items.FirstOrDefault(item => item.ID == IDToRemove);

                            if (itemToRemove is not null)
                            {
                                targetWhToRemoveItem.RemoveItem(itemToRemove);
                            }

                            Console.WriteLine("Товар удалён");
                            break;

                        case 3:
                            Console.WriteLine("Введите ID склада-отправителя: ");
                            if (!int.TryParse(Console.ReadLine(), out int sourceWhID) || wareHouses.FirstOrDefault(wh => wh.ID == sourceWhID) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            var sourceWh = wareHouses.First(wh => wh.ID == sourceWhID);

                            Console.WriteLine("Введите ID склада-получателя: ");
                            if(!int.TryParse(Console.ReadLine(), out int targetWhID) || wareHouses.FirstOrDefault(wh => wh.ID == targetWhID) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            var targetWh = wareHouses.First(wh => wh.ID == targetWhID);

                            Console.WriteLine("Введите ID товара: ");
                            if(!int.TryParse(Console.ReadLine(), out int transferItemID))
                            {
                                Console.WriteLine("Некорректный ID");
                                break;
                            }

                            var itemToTransfer = sourceWh.Items.FirstOrDefault(item => item.ID == transferItemID);

                            if (itemToTransfer is null)
                            {
                                Console.WriteLine($"Товар с ID {transferItemID} не найден на складе {targetWh.Name}");
                                break;
                            }

                            Console.WriteLine($"Введите колличество товара (Доступно: {itemToTransfer.Quantity}): ");
                            if (!int.TryParse(Console.ReadLine(), out int transferItemQuantity) || transferItemQuantity <= 0)
                            {
                                Console.WriteLine("Некорректный ввод");
                                break;
                            }
 
                            if(transferItemQuantity > itemToTransfer.Quantity)
                            {
                                Console.WriteLine($"Недостаточно товара. Вы пытаетесь переместить {transferItemQuantity}, но на складе есть только {itemToTransfer.Quantity}");
                                break;
                            }

                            itemToTransfer.TransferTo(targetWh, transferItemQuantity);

                            Console.WriteLine("Товар перемещён");
                            break;

                        case 4:
                            Console.WriteLine("Введите ID склада: ");
                            if(!int.TryParse(Console.ReadLine(), out int wareHouseIDToCreate) || wareHouses.FirstOrDefault(wh => wh.ID == wareHouseIDToCreate) is not null)
                            {
                                Console.WriteLine("Склад с таким ID уже существует");
                                break;
                            }

                            Console.WriteLine("Введите название склада: ");
                            string wareHouseName = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(wareHouseName))
                            {
                                Console.WriteLine("Некорректный ввод");
                                break;
                            }
                            Console.WriteLine("Введите адрес склада: ");
                            string wareHouseAddress = Console.ReadLine();
                            if(string.IsNullOrWhiteSpace(wareHouseAddress)) 
                            {
                                Console.WriteLine("Некорректный ввод");
                                break;
                            }

                            WareHouse newWareHouse = wareHouseFactory.CreateWareHouse(wareHouseIDToCreate, wareHouseName, wareHouseAddress);

                            wareHouses.Add(newWareHouse);

                            Console.WriteLine("Склад добавлен");
                            break;

                        case 5:
                            Console.WriteLine("Введите ID склада: ");
                            if (!int.TryParse(Console.ReadLine(), out int wareHouseIDToRemove))
                            {
                                Console.WriteLine("Некорректный ввод");
                                break;
                            }

                            WareHouse wareHouseToRemove = wareHouses.FirstOrDefault(wh => wh.ID == wareHouseIDToRemove);

                            if (wareHouseToRemove == null)
                            {
                                Console.WriteLine("Склад не найден");
                                break;
                            }

                            if(wareHouseToRemove.Items is not null && wareHouseToRemove.Items.Count > 0)
                            {
                                Console.WriteLine("Нельзя удалить склад пока в нём есть предметы");
                                break;
                            }

                            wareHouses.Remove(wareHouseToRemove);

                            Console.WriteLine("Склад удалён");
                            break;

                        case 6:
                            Console.WriteLine("Список складов: ");

                            foreach (WareHouse wareHouse in wareHouses)
                            {
                                Console.WriteLine($"ID: {wareHouse.ID} | Название: {wareHouse.Name} | Адрес: {wareHouse.Address}");
                            }

                            break;

                        case 7:
                            Console.WriteLine("Введите ID склада, в которым будет искаться товар: ");

                            if (!int.TryParse(Console.ReadLine(), out int whToSearchItem) || wareHouses.FirstOrDefault(wh => wh.ID == whToSearchItem) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            var targetWareHouse = wareHouses.First(wh => wh.ID == whToSearchItem);

                            if (targetWareHouse.Items is null || targetWareHouse.Items.Count == 0)
                            {
                                Console.WriteLine($"На складе ID: {targetWareHouse.ID} |  Название: {targetWareHouse.Name} | Адрес: {targetWareHouse.Address} нет товаров");
                                break;
                            }

                            Console.WriteLine($"Товары на складе ID: {targetWareHouse.ID} |  Название: {targetWareHouse.Name} | Адрес: {targetWareHouse.Address}");

                            foreach (var item in targetWareHouse.Items)
                            {
                                Console.WriteLine($"ID: {item.ID} | Название: {item.Name} | Категория: {item.Category} | Цена: {item.Price} | Кол-во: {item.Quantity}");

                                switch (item)
                                {
                                    case Electronics electronics:
                                        Console.WriteLine($"Гарантия: {electronics.WarrantyPeriod} мес.");
                                        break;

                                    case Food food:
                                        Console.WriteLine($"Срок годности: {food.ExpirationDate:dd.MM.yyyy}");
                                        break;


                                    case Furniture furniture:
                                        Console.WriteLine($"Габариты: {furniture.Dimesions}");
                                        break;
                                }
                            }
                            break;

                        case 8:
                            Console.WriteLine("Введите ID склада, для рассчёта стоимости товара: ");

                            if (!int.TryParse(Console.ReadLine(), out int whToTotalValue) || wareHouses.FirstOrDefault(wh => wh.ID == whToTotalValue) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            var whTotalValue = wareHouses.First(wh => wh.ID == whToTotalValue);

                            if (whTotalValue.Items is null || whTotalValue.Items.Count == 0)
                            {
                                Console.WriteLine($"На складе ID: {whTotalValue.ID} |  Название: {whTotalValue.Name} | Адрес: {whTotalValue.Address} нет товаров");
                                break;
                            }

                            Console.WriteLine($"Общая стоимость на складе {whTotalValue.Name} равна {whTotalValue.GetTotalValue()} руб.");
                            break;

                        case 9:
                            Console.WriteLine("Введите ID склада, для рассчёта стоимости товара: ");

                            if (!int.TryParse(Console.ReadLine(), out int whToSearchCategory) || wareHouses.FirstOrDefault(wh => wh.ID == whToSearchCategory) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            var whToSearch = wareHouses.First(wh=>wh.ID == whToSearchCategory);

                            Console.WriteLine("Введите категорию: ");
                            string searchCategory = Console.ReadLine();
                            if(string.IsNullOrWhiteSpace(searchCategory))
                            {
                                Console.WriteLine("Некорректный ввод.");
                                break;
                            }

                            whToSearch.GetItemsByCategory(searchCategory);
                            break;

                        case 10:
                            Console.WriteLine("Введите ID склада, на котором находится товар: ");
                            if (!int.TryParse(Console.ReadLine(), out int whIdForCost) || wareHouses.FirstOrDefault(wh => wh.ID == whIdForCost) is null)
                            {
                                Console.WriteLine("Склад не найден.");
                                break;
                            }

                            var whForCost = wareHouses.First(wh => wh.ID == whIdForCost);

                            if(whForCost.Items is null || whForCost.Items.Count ==0)
                            {
                                Console.WriteLine("На этом складе нет товаров.");
                                break;
                            }

                            Console.WriteLine("Введите ID товара для расчета стоимости хранения: ");
                            if (!int.TryParse(Console.ReadLine(), out int itemIdForCost))
                            {
                                Console.WriteLine("Некорректный ввод.");
                                break;
                            }

                            var targetItem = whForCost.Items.FirstOrDefault(item => item.ID == itemIdForCost);

                            if(targetItem is null)
                            {
                                Console.WriteLine("Товар с таким ID не найден на этом складе.");
                                break;
                            }

                            Console.WriteLine("Введите срок хранения");
                            if (!int.TryParse(Console.ReadLine(), out int days))
                            {
                                Console.WriteLine("Некорректный ввод.");
                                break;
                            }

                            Console.WriteLine($"Стоимость хранения {targetItem.Name} равна {targetItem.CalculateStorageCost(days)} руб.");
                            break;

                        case 11:
                            isRuning = false;
                            break;

                        default:
                            Console.WriteLine("Некорректный ввод.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка {ex.Message}");
                }
            }
        }

        static void AddItemToWH(List<WareHouse> wareHouses, WareHouseItemFactory itemFactory)
        {
            Console.WriteLine("Введите ID склада, в который добавиться товар: ");
            if (!int.TryParse(Console.ReadLine(), out int whIDToCreateItem) || wareHouses.FirstOrDefault(wh => wh.ID == whIDToCreateItem) is null)
            {
                Console.WriteLine("Склад не найден.");
                break;
            }

            var targetWhToCreateItem = wareHouses.First(wh => wh.ID == whIDToCreateItem);

            Console.WriteLine("Выбирете тип предмета:");
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║ 1. Электроника                         ║");
            Console.WriteLine("║ 2. Еда                                 ║");
            Console.WriteLine("║ 3. Мебель                              ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            if (!int.TryParse(Console.ReadLine(), out int itemTypeChoice))
            {
                Console.WriteLine("Некорректный ввод.");
                break;
            }

            Console.WriteLine("Введите ID:");
            if (!int.TryParse(Console.ReadLine(), out int itemID))
            {
                Console.WriteLine("Некорректный ввод.");
                break;
            }

            Console.WriteLine("Введите название:");
            string itemName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(itemName))
            {
                Console.WriteLine("Некорректный ввод.");
                break;
            }

            Console.WriteLine("Введите категорию:");
            string itemCategory = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(itemCategory))
            {
                Console.WriteLine("Некорректный ввод.");
                break;
            }

            Console.WriteLine("Введите цену:");
            if (!decimal.TryParse(Console.ReadLine(), out decimal itemPrice))
            {
                Console.WriteLine("Некорректный ввод.");
                break;
            }

            Console.WriteLine("Введите количество:");
            if (!int.TryParse(Console.ReadLine(), out int itemQuantity))
            {
                Console.WriteLine("Некорректный ввод .");
                break;
            }

            DateTime itemLastUpdate = DateTime.Now;

            try
            {
                switch (itemTypeChoice)
                {
                    case 1:
                        while (true)
                        {
                            Console.WriteLine("Введите гарантийный срок (мес.):");

                            if (int.TryParse(Console.ReadLine(), out int electronicsWarrantyPeriod) || electronicsWarrantyPeriod >= 0)
                            {
                                targetWhToCreateItem.AddItem(itemFactory.CreateItem(ItemType.Electronics, itemID, itemName, itemCategory, itemPrice, itemQuantity, itemLastUpdate, electronicsWarrantyPeriod));

                                Console.WriteLine("Товар добавлен.");

                                break;
                            }
                            Console.WriteLine("Некорректный ввод.");
                        }
                        break;

                    case 2:
                        DateTime foodExpirationDate;
                        while (true)
                        {
                            Console.WriteLine("Введите дату окончания срока (в формате ДД.ММ.ГГГГ, например 15.08.2026):");
                            if (DateTime.TryParse(Console.ReadLine(), out foodExpirationDate))
                            {
                                targetWhToCreateItem.AddItem(itemFactory.CreateItem(ItemType.Food, itemID, itemName, itemCategory, itemPrice, itemQuantity, itemLastUpdate, foodExpirationDate));

                                Console.WriteLine("Товар добавлен.");

                                break;
                            }
                            Console.WriteLine("Некорректный ввод.");
                        }
                        break;

                    case 3:
                        int lenght = 0, width = 0, height = 0;

                        while (true)
                        {
                            Console.WriteLine("Введите габариты товара в формате ДлинахШиринахВысота");

                            string dimensionsInput = Console.ReadLine()?.ToLower().Replace(" ", " ");

                            string[] parts = dimensionsInput.Split(new char[] { 'x', 'х' });

                            if (parts.Length == 3 && int.TryParse(parts[0], out lenght) && int.TryParse(parts[1], out width) && int.TryParse(parts[3], out height))
                            {
                                if (lenght > 0 && width > 0 && height > 0)
                                {
                                    break;
                                }
                            }
                            Console.WriteLine("Некорректный ввод.");
                        }

                        string furnitureDimession = $"{lenght}x{width}x{height}";

                        targetWhToCreateItem.AddItem(itemFactory.CreateItem(ItemType.Furniture, itemID, itemName, itemCategory, itemPrice, itemQuantity, itemLastUpdate, furnitureDimession));

                        Console.WriteLine("Товар добавлен.");
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


