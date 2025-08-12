using System;
using System.Collections.Generic;

// --- Custom Exceptions ---
public class DuplicateItemException : Exception
{
    public DuplicateItemException(string message) : base(message) { }
}

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message) { }
}

public class InvalidQuantityException : Exception
{
    public InvalidQuantityException(string message) : base(message) { }
}

// --- Marker Interface  for Inventory Items ---
public interface IInventoryItem
{
    int Id { get; }
    string Name { get; }
    int Quantity { get; set; }
}

// --- Electronic Item ---
public class ElectronicItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public string Brand { get; }
    public int WarrantyMonths { get; }

    public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
    {
        if (quantity < 0)
            throw new InvalidQuantityException("Quantity cannot be negative");

        Id = id;
        Name = name;
        Quantity = quantity;
        Brand = brand;
        WarrantyMonths = warrantyMonths;
    }

    public override string ToString()
    {
        return $"Electronic Item: [Id = {Id}, Name = {Name}, Quantity = {Quantity}, Brand = {Brand}, Warranty Months = {WarrantyMonths}]";
    }
}

// --- Grocery Item ---
public class GroceryItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; }

    public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
    {
        if (quantity < 0)
            throw new InvalidQuantityException("Quantity cannot be negative");

        Id = id;
        Name = name;
        Quantity = quantity;
        ExpiryDate = expiryDate;
    }

    public override string ToString()
    {
        return $"Grocery Item: [Id = {Id}, Name = {Name}, Quantity = {Quantity}, Expiry Date = {ExpiryDate.ToShortDateString()}]";
    }
}

// --- Inventory Repository ---
public class InventoryRepository<T> where T : IInventoryItem
{
    private Dictionary<int, T> _items = new Dictionary<int, T>();

    public void AddItem(T item)
    {
        if (_items.ContainsKey(item.Id))
            throw new DuplicateItemException($"Item with ID {item.Id} already exists.");

        _items.Add(item.Id, item);
    }

    public T GetItemById(int id)
    {
        if (!_items.TryGetValue(id, out T item))
            throw new ItemNotFoundException($"Item with ID {id} not found.");
        return item;
    }

    public void RemoveItem(int id)
    {
        if (!_items.ContainsKey(id))
            throw new ItemNotFoundException($"Item with ID {id} not found.");
        _items.Remove(id);
    }

    public void UpdateQuantity(int id, int newQuantity)
    {
        if (newQuantity < 0)
            throw new InvalidQuantityException("Quantity cannot be negative");

        var item = GetItemById(id);
        item.Quantity = newQuantity;
    }

    public List<T> GetAllItems()
    {
        return new List<T>(_items.Values);
    }
}

// --- Warehouse Manager ---
public class WarehouseManager
{
    private InventoryRepository<ElectronicItem> _electronics = new InventoryRepository<ElectronicItem>();
    private InventoryRepository<GroceryItem> _groceries = new InventoryRepository<GroceryItem>();

    public void SeedData()
    {
        try
        {
            _electronics.AddItem(new ElectronicItem(1, "Laptop", 10, "Dell", 24));
            _electronics.AddItem(new ElectronicItem(2, "Mobile Phone", 20, "iPhone", 12));
            _electronics.AddItem(new ElectronicItem(3, "Headphone", 50, "Sony", 6));

            _groceries.AddItem(new GroceryItem(101, "Milk", 30, DateTime.Now.AddDays(7)));
            _groceries.AddItem(new GroceryItem(102, "Bread", 15, DateTime.Now.AddDays(3)));
            _groceries.AddItem(new GroceryItem(103, "Eggs", 60, DateTime.Now.AddDays(14)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding data: {ex.Message}");
        }
    }

    public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
    {
        var items = repo.GetAllItems();
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }

    public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
    {
        try
        {
            if (quantity <= 0)
                throw new InvalidQuantityException("Quantity to increase must be positive");

            var item = repo.GetItemById(id);
            int newQuantity = item.Quantity + quantity;
            repo.UpdateQuantity(id, newQuantity);
            Console.WriteLine($"Increased stock of '{item.Name}' (ID: {id}) by {quantity}. New quantity: {newQuantity}.");
        }
        catch (DuplicateItemException dex)
        {
            Console.WriteLine($"Duplicate item error: {dex.Message}");
        }
        catch (ItemNotFoundException infex)
        {
            Console.WriteLine($"Item not found error: {infex.Message}");
        }
        catch (InvalidQuantityException iqex)
        {
            Console.WriteLine($"Invalid quantity error: {iqex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
    {
        try
        {
            repo.RemoveItem(id);
            Console.WriteLine($"Item with ID {id} removed successfully.");
        }
        catch (ItemNotFoundException infex)
        {
            Console.WriteLine($"Item not found error: {infex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    public InventoryRepository<ElectronicItem> Electronics => _electronics;
    public InventoryRepository<GroceryItem> Groceries => _groceries;
}

// --- Main Program ---
class Program
{
    static void Main()
    {
        var manager = new WarehouseManager();

        // Seed initial data
        manager.SeedData();

        Console.WriteLine("All Grocery Items:");
        manager.PrintAllItems(manager.Groceries);
        Console.WriteLine();

        Console.WriteLine("All Electronic Items:");
        manager.PrintAllItems(manager.Electronics);
        Console.WriteLine();

        // Testing duplicate item addition
        Console.WriteLine("Trying to add duplicate Electronic Item (ID=1):");
        try
        {
            manager.Electronics.AddItem(new ElectronicItem(1, "Tablet", 5, "Apple", 12));
        }
        catch (DuplicateItemException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.WriteLine();

        // Testing removing non-existent item
        Console.WriteLine("Trying to remove non-existent Grocery Item (ID=999):");
        manager.RemoveItemById(manager.Groceries, 999);
        Console.WriteLine();

        // Testing invalid quantity update
        Console.WriteLine("Trying to update quantity with invalid value (-5) for Electronic Item ID=2:");
        try
        {
            manager.Electronics.UpdateQuantity(2, -5);
        }
        catch (InvalidQuantityException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        Console.WriteLine();

        // Testing invalid increase quantity
        Console.WriteLine("Trying to increase stock with invalid quantity (0) for Electronic Item ID=2:");
        manager.IncreaseStock(manager.Electronics, 2, 0);
        Console.WriteLine();

        // Increase stock normally to show success
        manager.IncreaseStock(manager.Electronics, 2, 10);
        Console.WriteLine();

        Console.WriteLine("Final Electronic Items:");
        manager.PrintAllItems(manager.Electronics);
    }
}