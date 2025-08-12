using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// =============================
// Custom Exceptions
// =============================
public class InvalidScoreFormatException : Exception
{
    public InvalidScoreFormatException(string message) : base(message) { }
}

public class MissingFieldException : Exception
{
    public MissingFieldException(string message) : base(message) { }
}

public class DuplicateStudentIdException : Exception
{
    public DuplicateStudentIdException(string message) : base(message) { }
}

public class InvalidScoreValueException : Exception
{
    public InvalidScoreValueException(string message) : base(message) { }
}

// =============================
// Student Class
// =============================
public class Student
{
    public int Id { get; }
    public string FullName { get; }
    public int Score { get; }

    public Student(int id, string fullName, int score)
    {
        if (id <= 0)
            throw new ArgumentException("Student ID must be positive", nameof(id));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));
        if (score < 0 || score > 100)
            throw new InvalidScoreValueException($"Score must be between 0 and 100 (got {score})");

        Id = id;
        FullName = fullName;
        Score = score;
    }

    public string GetGrade()
    {
        return Score switch
        {
            >= 80 and <= 100 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _ => "F"
        };
    }

    public override string ToString()
    {
        return $"{Id},{FullName},{Score},{GetGrade()}";
    }

    public string ToDisplayString()
    {
        return $"{FullName} (ID: {Id}): Score = {Score}, Grade = {GetGrade()}";
    }
}

// =============================
// StudentResultProcessor
// =============================
public class StudentResultProcessor
{
    public List<Student> ReadStudentsFromFile(string inputFilePath)
    {
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException($"Input file not found: {inputFilePath}");

        var students = new List<Student>();
        var studentIds = new HashSet<int>();
        int lineNumber = 0;

        foreach (string line in File.ReadLines(inputFilePath))
        {
            lineNumber++;

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                string[] parts = line.Split(',');

                // Validate field count
                if (parts.Length < 3)
                    throw new MissingFieldException($"Line {lineNumber}: Expected 3 fields, got {parts.Length}");

                // Parse fields with validation
                if (!int.TryParse(parts[0].Trim(), out int id) || id <= 0)
                    throw new InvalidScoreFormatException($"Line {lineNumber}: Invalid student ID format");

                string fullName = parts[1].Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    throw new MissingFieldException($"Line {lineNumber}: Student name cannot be empty");

                if (!int.TryParse(parts[2].Trim(), out int score))
                    throw new InvalidScoreFormatException($"Line {lineNumber}: Invalid score format");

                // Check for duplicate IDs
                if (studentIds.Contains(id))
                    throw new DuplicateStudentIdException($"Line {lineNumber}: Duplicate student ID {id}");

                var student = new Student(id, fullName, score);
                students.Add(student);
                studentIds.Add(id);
            }
            catch (InvalidScoreValueException ex)
            {
                throw new InvalidScoreFormatException($"Line {lineNumber}: {ex.Message}");
            }
        }

        if (students.Count == 0)
            throw new Exception("No valid student records found in the input file");

        return students;
    }

    public void WriteReportToFile(List<Student> students, string outputFilePath)
    {
        if (students == null || students.Count == 0)
            throw new ArgumentException("Student list cannot be empty", nameof(students));

        try
        {
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                // Write header
                writer.WriteLine("StudentID,FullName,Score,Grade");

                // Write student records sorted by score (descending)
                foreach (var student in students.OrderByDescending(s => s.Score))
                {
                    writer.WriteLine(student);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw new Exception($"No permission to write to output file: {outputFilePath}");
        }
        catch (PathTooLongException)
        {
            throw new Exception($"Output file path is too long: {outputFilePath}");
        }
    }

    public void GenerateSummaryReport(List<Student> students, string summaryFilePath)
    {
        var summary = new Dictionary<string, int>
        {
            ["A"] = 0,
            ["B"] = 0,
            ["C"] = 0,
            ["D"] = 0,
            ["F"] = 0
        };

        foreach (var student in students)
        {
            summary[student.GetGrade()]++;
        }

        using (StreamWriter writer = new StreamWriter(summaryFilePath))
        {
            writer.WriteLine("Grade,Count");
            foreach (var entry in summary)
            {
                writer.WriteLine($"{entry.Key},{entry.Value}");
            }

            writer.WriteLine($"\nTotal Students,{students.Count}");
            writer.WriteLine($"Highest Score,{students.Max(s => s.Score)}");
            writer.WriteLine($"Lowest Score,{students.Min(s => s.Score)}");
            writer.WriteLine($"Average Score,{students.Average(s => s.Score):F2}");
        }
    }
}

// =============================
// Main Application
// =============================

class Program
{
    static void Main()
    {
        // Get the directory where the executable is running
        string executablePath = AppDomain.CurrentDomain.BaseDirectory;

        // Set file paths - now they'll be relative to the executable location
        string inputFilePath = Path.Combine(executablePath, "students.txt");
        string outputFilePath = Path.Combine(executablePath, "report.txt");
        string summaryFilePath = Path.Combine(executablePath, "summary.csv");

        try
        {
            // If input file doesn't exist, create a sample one
            if (!File.Exists(inputFilePath))
            {
                CreateSampleInputFile(inputFilePath);
                Console.WriteLine("Sample input file created at: " + inputFilePath);
            }

            var processor = new StudentResultProcessor();

            // Read and process student data
            List<Student> students = processor.ReadStudentsFromFile(inputFilePath);

            // Generate reports
            processor.WriteReportToFile(students, outputFilePath);
            processor.GenerateSummaryReport(students, summaryFilePath);

            Console.WriteLine($"\nSuccessfully processed {students.Count} student records");
            Console.WriteLine($"Detailed report saved to: {outputFilePath}");
            Console.WriteLine($"Summary statistics saved to: {summaryFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void CreateSampleInputFile(string path)
    {
        string[] sampleData = {
            "1,John Smith,85",
            "2,Jane Doe,72",
            "3,Michael Johnson,91",
            "4,Emily Williams,68",
            "5,Robert Brown,55"
        };
        File.WriteAllLines(path, sampleData);
    }
}