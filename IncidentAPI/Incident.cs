using System;
using System.Collections.Generic;
using System.Linq;

namespace IncidentAPI.Models
{
    /// <summary>
    /// Represents an incident with urgency calculation capabilities
    /// </summary>
    public class Incident
    {
        private string _title = string.Empty;
        private string _severity = string.Empty;
        private DateTime _dateReported;

        // Properties with validation
        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be null or empty");
                if (value.Length > 200)
                    throw new ArgumentException("Title cannot exceed 200 characters");
                _title = value.Trim();
            }
        }

        public string Severity
        {
            get => _severity;
            set
            {
                var validSeverities = new[] { "Low", "Medium", "High" };
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Severity cannot be null or empty");
                if (!validSeverities.Contains(value, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException($"Severity must be one of: {string.Join(", ", validSeverities)}");
                _severity = CapitalizeFirstLetter(value.Trim());
            }
        }

        public DateTime DateReported
        {
            get => _dateReported;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Date reported cannot be in the future");
                if (value < DateTime.Now.AddYears(-10))
                    throw new ArgumentException("Date reported cannot be more than 10 years ago");
                _dateReported = value;
            }
        }

        // Read-only calculated properties
        public TimeSpan Age => DateTime.Now - DateReported;
        public int AgeDays => (int)Age.TotalDays;
        public int AgeHours => (int)Age.TotalHours;

        // Constructors
        public Incident()
        {
            DateReported = DateTime.Now;
        }

        public Incident(string title, string severity, DateTime dateReported)
        {
            Title = title;
            Severity = severity;
            DateReported = dateReported;
        }

        public Incident(string title, string severity) : this(title, severity, DateTime.Now)
        {
        }

        /// <summary>
        /// Calculates incident urgency based on severity level and age
        /// Business Rules:
        /// - High severity: Base 8, escalates quickly (hours)
        /// - Medium severity: Base 5, escalates over days  
        /// - Low severity: Base 2, escalates over weeks
        /// - Older incidents get higher priority
        /// </summary>
        /// <returns>Urgency level from 1 (lowest) to 10 (highest)</returns>
        public int CalculateUrgency()
        {
            // Base urgency from severity
            int baseUrgency = Severity switch
            {
                "High" => 8,    // High severity starts at 8
                "Medium" => 5,  // Medium severity starts at 5
                "Low" => 2,     // Low severity starts at 2
                _ => 1          // Unknown severity gets lowest priority
            };

            // Age-based urgency multiplier
            double ageMultiplier = CalculateAgeMultiplier();

            // Calculate final urgency (capped at 10)
            int finalUrgency = Math.Min(10, (int)Math.Ceiling(baseUrgency * ageMultiplier));

            return Math.Max(1, finalUrgency); // Ensure minimum urgency of 1
        }

        /// <summary>
        /// Gets a descriptive urgency level
        /// </summary>
        /// <returns>Descriptive urgency text</returns>
        public string GetUrgencyDescription()
        {
            int urgency = CalculateUrgency();
            return urgency switch
            {
                >= 9 => "Critical - Immediate Action Required",
                >= 7 => "High - Action Required Today",
                >= 5 => "Medium - Action Required This Week",
                >= 3 => "Low - Action Required Soon",
                _ => "Minimal - Action When Convenient"
            };
        }

        /// <summary>
        /// Determines if incident requires immediate attention
        /// </summary>
        /// <returns>True if urgent (urgency >= 7)</returns>
        public bool IsUrgent()
        {
            return CalculateUrgency() >= 7;
        }

        // Private helper methods
        private double CalculateAgeMultiplier()
        {
            double hours = Age.TotalHours;

            return Severity switch
            {
                "High" => hours switch
                {
                    <= 1 => 1.0,      // Recent high severity
                    <= 4 => 1.2,      // 1-4 hours old
                    <= 24 => 1.4,     // 4-24 hours old  
                    <= 48 => 1.6,     // 1-2 days old
                    _ => 2.0           // Very old high severity
                },
                "Medium" => hours switch
                {
                    <= 24 => 1.0,     // Recent medium severity
                    <= 72 => 1.2,     // 1-3 days old
                    <= 168 => 1.4,    // 3-7 days old
                    _ => 1.6           // Very old medium severity
                },
                "Low" => hours switch
                {
                    <= 168 => 1.0,    // Recent low severity (1 week)
                    <= 720 => 1.2,    // 1-4 weeks old
                    _ => 1.4           // Very old low severity
                },
                _ => 1.0
            };
        }

        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return char.ToUpper(input[0]) + input[1..].ToLower();
        }

        // Validation method
        public List<string> Validate()
        {
            var errors = new List<string>();

            try { var _ = Title; }
            catch (ArgumentException ex) { errors.Add($"Title: {ex.Message}"); }

            try { var _ = Severity; }
            catch (ArgumentException ex) { errors.Add($"Severity: {ex.Message}"); }

            try { var _ = DateReported; }
            catch (ArgumentException ex) { errors.Add($"DateReported: {ex.Message}"); }

            return errors;
        }

        public bool IsValid() => !Validate().Any();

        // Override methods for better object representation
        public override string ToString()
        {
            return $"[{Severity}] {Title} (Reported: {DateReported:yyyy-MM-dd}, Age: {AgeDays}d, Urgency: {CalculateUrgency()})";
        }
    }
}

// Simple demonstration class
namespace IncidentAPI.Models
{
    public class IncidentDemo
    {
        public static void RunDemo()
        {
            Console.WriteLine("=== INCIDENT CLASS DEMONSTRATION ===\n");

            try
            {
                // 1. Create incidents with different scenarios
                Console.WriteLine("1. Creating incidents:");

                var incident1 = new Incident("Server Outage", "High", DateTime.Now.AddHours(-2));
                var incident2 = new Incident("Database Slow", "Medium", DateTime.Now.AddDays(-3));
                var incident3 = new Incident("UI Bug", "Low", DateTime.Now.AddDays(-7));

                Console.WriteLine($"✅ {incident1}");
                Console.WriteLine($"✅ {incident2}");
                Console.WriteLine($"✅ {incident3}");

                // 2. Demonstrate urgency calculation
                Console.WriteLine("\n2. Urgency calculations:");
                var incidents = new[] { incident1, incident2, incident3 };

                foreach (var incident in incidents)
                {
                    Console.WriteLine($"\n📊 {incident.Title}:");
                    Console.WriteLine($"   • Urgency Score: {incident.CalculateUrgency()}/10");
                    Console.WriteLine($"   • Description: {incident.GetUrgencyDescription()}");
                    Console.WriteLine($"   • Is Urgent: {(incident.IsUrgent() ? "YES" : "NO")}");
                    Console.WriteLine($"   • Age: {incident.AgeDays} days, {incident.AgeHours} hours");
                }

                // 3. Validation examples
                Console.WriteLine("\n3. Validation examples:");

                var validIncident = new Incident("Valid Test", "Medium");
                Console.WriteLine($"✅ Valid incident: {validIncident.IsValid()}");

                try
                {
                    var invalidIncident = new Incident("", "High"); // Empty title
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"❌ Validation caught: {ex.Message}");
                }

                // 4. Age-based urgency demonstration
                Console.WriteLine("\n4. Age-based urgency escalation:");

                var oldHighIncident = new Incident("Critical Issue", "High", DateTime.Now.AddDays(-2));
                var oldMediumIncident = new Incident("Performance Issue", "Medium", DateTime.Now.AddDays(-10));

                Console.WriteLine($"⏰ Old High Incident ({oldHighIncident.AgeDays} days): Urgency {oldHighIncident.CalculateUrgency()}");
                Console.WriteLine($"⏰ Old Medium Incident ({oldMediumIncident.AgeDays} days): Urgency {oldMediumIncident.CalculateUrgency()}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error: {ex.Message}");
            }

            Console.WriteLine("\n=== DEMONSTRATION COMPLETE ===");
        }
    }
}
