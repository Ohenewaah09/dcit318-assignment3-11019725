using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCareSystem
{
    public class Repository<T>
    {
        private readonly List<T> Items = new List<T>();

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Items.Add(item);
        }

        public List<T> GetAll()
        {
            return new List<T>(Items);
        }

        public T? GetById(Func<T, bool> predicate)
        {
            return Items.FirstOrDefault(predicate);
        }

        public bool Remove(Func<T, bool> predicate)
        {
            var toRemove = Items.FirstOrDefault(predicate);
            if (toRemove != null)
            {
                Items.Remove(toRemove);
                return true;
            }
            return false;
        }
    }

    public class Patient
    {
        public int Id { get; }
        public string Name { get; }
        public int Age { get; }
        public string Gender { get; }

        public Patient(int Id, string Name, int Age, string Gender)
        {
            this.Id = Id;
            this.Name = Name ?? throw new ArgumentNullException(nameof(Name));
            this.Age = Age;
            this.Gender = Gender ?? throw new ArgumentNullException(nameof(Gender));
        }

        public override string ToString()
        {
            return $"Patient {{ Id = {Id}, Name = {Name}, Age = {Age}, Gender = {Gender} }}";
        }
    }

    public class Prescription
    {
        public int Id { get; }
        public int PatientId { get; }
        public string MedicationName { get; }
        public DateTime DateIssued { get; }

        public Prescription(int Id, int PatientId, string MedicationName, DateTime DateIssued)
        {
            this.Id = Id;
            this.PatientId = PatientId;
            this.MedicationName = MedicationName ?? throw new ArgumentNullException(nameof(MedicationName));
            this.DateIssued = DateIssued;
        }

        public override string ToString()
        {
            return $"Prescription {{ Id = {Id}, PatientId = {PatientId}, Medication = {MedicationName}, DateIssued = {DateIssued:yyyy-MM-dd} }}";
        }
    }

    public class HealthSystemApp
    {
        private readonly Repository<Patient> _patientRepo;
        private readonly Repository<Prescription> _prescriptionRepo;
        private Dictionary<int, List<Prescription>> _prescriptionMap;

        public HealthSystemApp()
        {
            _patientRepo = new Repository<Patient>();
            _prescriptionRepo = new Repository<Prescription>();
        }

        public void SeedData()
        {
            _patientRepo.Add(new Patient(1, "Kofi Manu", 34, "Male"));
            _patientRepo.Add(new Patient(2, "Ama Serwa", 28, "Female"));
            _patientRepo.Add(new Patient(3, "Kwame Nkrumah", 67, "Male"));

            _prescriptionRepo.Add(new Prescription(1, 1, "Amoxicillin", DateTime.UtcNow.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(2, 1, "Ibuprofen", DateTime.UtcNow.AddDays(-5)));
            _prescriptionRepo.Add(new Prescription(3, 2, "Metformin", DateTime.UtcNow.AddDays(-20)));
            _prescriptionRepo.Add(new Prescription(4, 3, "Lisinopril", DateTime.UtcNow.AddDays(-15)));
            _prescriptionRepo.Add(new Prescription(5, 2, "Atorvastatin", DateTime.UtcNow.AddDays(-7)));
        }

        public void BuildPrescriptionMap()
        {
            _prescriptionMap = _prescriptionRepo.GetAll()
                .GroupBy(p => p.PatientId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public void PrintAllPatients()
        {
            var patients = _patientRepo.GetAll();
            foreach (var p in patients)
            {
                Console.WriteLine(p);
            }
        }

        public void PrintPrescriptionForPatient(int patientId)
        {
            if (_prescriptionMap.TryGetValue(patientId, out var prescriptions))
            {
                Console.WriteLine($"Prescriptions for Patient ID {patientId}:");
                foreach (var pres in prescriptions)
                {
                    Console.WriteLine(pres);
                }
            }
            else
            {
                Console.WriteLine($"No prescriptions found for Patient ID {patientId}.");
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new HealthSystemApp();

            app.SeedData();
            app.BuildPrescriptionMap();

            Console.WriteLine("All Patients:");
            app.PrintAllPatients();

            Console.WriteLine("\nEnter a Patient ID to view prescriptions:");
            if (int.TryParse(Console.ReadLine(), out int pid))
            {
                Console.WriteLine();
                app.PrintPrescriptionForPatient(pid);
            }
            else
            {
                Console.WriteLine("Invalid Patient ID.");
            }
        }
    }
}
