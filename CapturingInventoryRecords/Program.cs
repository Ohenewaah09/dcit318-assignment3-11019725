using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

// ======================
// Marker Interface
// ======================
public interface IInventoryEntity
{
    int Id { get; }
}

// ======================
// Inventory Item Record
// ======================
public record InventoryItem(
    int Id,
    string Name,
    int Quantity,
    [property: JsonConverter(typeof(DateTimeJsonConverter))] DateTime DateAdded
) : IInventoryEntity;

// Custom JSON converter for DateTime to handle serialization
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("o")); // ISO 8601 format
    }
}

// ======================
// Generic Inventory Logger
// ======================
public class InventoryLogger<T> where T : IInventoryEntity
{
    private readonly List<T> _log = new();
    private readonly string _filePath;

    public InventoryLogger(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public void Add(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        // Check for duplicate IDs
        if (_log.Exists(x => x.Id == item.Id))
            throw new InvalidOperationException($"Item with ID {item.Id} already exists.");

        _log.Add(item);
    }

    public List<T> GetAll() => new(_log);

    public void SaveToFile()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_log, options);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex) when (
            ex is UnauthorizedAccessException or
            PathTooLongException or
            DirectoryNotFoundException or
            IOException)
        {
            throw new InventoryFileException($"Failed to save inventory data: {ex.Message}", ex);
        }
    }

    public void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Inventory file not found", _filePath);

            string json = File.ReadAllText(_filePath);
            var loadedItems = JsonSerializer.Deserialize<List<T>>(json);

            if (loadedItems != null)
            {
                _log.Clear();
                _log.AddRange(loadedItems);
            }
        }
        catch (JsonException ex)
        {
            throw new InventoryFileException($"Failed to parse inventory data: {ex.Message}", ex);
        }
        catch (Exception ex) when (
            ex is UnauthorizedAccessException or
            FileNotFoundException or
            PathTooLongException or
            DirectoryNotFoundException or
            IOException)
        {
            throw new InventoryFileException($"Failed to load inventory data: {ex.Message}", ex);
        }
    }
}

// ======================
// Custom Exceptions
// ======================
public class InventoryFileException : Exception
{
    public InventoryFileException(string message) : base(message) { }
    public InventoryFileException(string message, Exception inner) : base(message, inner) { }
}

// ======================
// Application Layer
// ======================
public class InventoryApp
{
    private readonly InventoryLogger<InventoryItem> _logger;

    public InventoryApp(string filePath)
    {
        _logger = new InventoryLogger<InventoryItem>(filePath);
    }

    public void SeedSampleData()
    {
        try
        {
            _logger.Add(new InventoryItem(1, "Laptop", 10, DateTime.Now.AddDays(-30)));
            _logger.Add(new InventoryItem(2, "Mouse", 50, DateTime.Now.AddDays(-15)));
            _logger.Add(new InventoryItem(3, "Keyboard", 25, DateTime.Now.AddDays(-7)));
            _logger.Add(new InventoryItem(4, "Monitor", 15, DateTime.Now.AddDays(-3)));
            _logger.Add(new InventoryItem(5, "Headphones", 30, DateTime.Now));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding data: {ex.Message}");
        }
    }

    public void SaveData()
    {
        try
        {
            _logger.SaveToFile();
            Console.WriteLine("Data saved successfully.");
        }
        catch (InventoryFileException ex)
        {
            Console.WriteLine($"Error saving data: {ex.Message}");
        }
    }

    public void LoadData()
    {
        try
        {
            _logger.LoadFromFile();
            Console.WriteLine("Data loaded successfully.");
        }
        catch (InventoryFileException ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
    }

    public void PrintAllItems()
    {
        var items = _logger.GetAll();
        if (items.Count == 0)
        {
            Console.WriteLine("No inventory items found.");
            return;
        }

        Console.WriteLine("Current Inventory:");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("ID\tName\t\tQty\tDate Added");
        Console.WriteLine("--------------------------------------------------");

        foreach (var item in items)
        {
            Console.WriteLine($"{item.Id}\t{item.Name}\t\t{item.Quantity}\t{item.DateAdded:yyyy-MM-dd}");
        }
    }
}

// ======================
// Main Program
// ======================
class Program
{
    static void Main()
    {
        string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "InventorySystem",
            "inventory.json");

        Console.WriteLine("Inventory Management System");
        Console.WriteLine("===========================");

        // Create and initialize the app
        var app = new InventoryApp(filePath);

        // Seed and save data
        Console.WriteLine("\nSeeding sample data...");
        app.SeedSampleData();
        app.SaveData();

        // Simulate new session
        Console.WriteLine("\nSimulating new session...");
        var newApp = new InventoryApp(filePath);

        // Load and display data
        Console.WriteLine("\nLoading saved data...");
        newApp.LoadData();
        newApp.PrintAllItems();
    }
}