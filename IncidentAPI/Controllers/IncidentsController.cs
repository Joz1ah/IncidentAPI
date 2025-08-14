using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IncidentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        // In-memory storage for incidents
        private static readonly List<Incident> _incidents = new List<Incident>();

        [HttpPost]
        public IActionResult CreateIncident([FromBody] CreateIncidentRequest request)
        {
            // 1. Input validation
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid input",
                    details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // 2. Severity validation
            var validSeverities = new[] { "Low", "Medium", "High" };
            if (!validSeverities.Contains(request.Severity, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    error = "Invalid severity level",
                    message = "Severity must be one of: Low, Medium, High",
                    provided = request.Severity
                });
            }

            // 3. Duplicate detection (same title and description within 24 hours)
            var cutoffTime = DateTime.UtcNow.AddHours(-24);
            var isDuplicate = _incidents.Any(i =>
                string.Equals(i.Title?.Trim(), request.Title?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(i.Description?.Trim(), request.Description?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                i.CreatedAt > cutoffTime);

            if (isDuplicate)
            {
                return Conflict(new
                {
                    error = "Duplicate incident detected",
                    message = "An incident with the same title and description was submitted within the last 24 hours"
                });
            }

            // 4. Create new incident
            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                Title = request.Title?.Trim(),
                Description = request.Description?.Trim(),
                Severity = CapitalizeSeverity(request.Severity),
                CreatedAt = DateTime.UtcNow,
                Status = "Open"
            };

            _incidents.Add(incident);

            // 5. Return created incident
            return CreatedAtAction(
                nameof(GetIncident),
                new { id = incident.Id },
                new IncidentResponse(incident)
            );
        }

        [HttpGet("{id}")]
        public IActionResult GetIncident(Guid id)
        {
            var incident = _incidents.FirstOrDefault(i => i.Id == id);
            if (incident == null)
            {
                return NotFound(new { error = "Incident not found" });
            }

            return Ok(new IncidentResponse(incident));
        }

        [HttpGet]
        public IActionResult GetIncidents()
        {
            var response = _incidents
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new IncidentResponse(i))
                .ToList();

            return Ok(new { incidents = response, count = response.Count });
        }

        // Edge case: Reset incidents (for testing)
        [HttpDelete("reset")]
        public IActionResult ResetIncidents()
        {
            _incidents.Clear();
            return Ok(new { message = "All incidents cleared" });
        }

        private static string CapitalizeSeverity(string severity)
        {
            return severity switch
            {
                var s when s.Equals("low", StringComparison.OrdinalIgnoreCase) => "Low",
                var s when s.Equals("medium", StringComparison.OrdinalIgnoreCase) => "Medium",
                var s when s.Equals("high", StringComparison.OrdinalIgnoreCase) => "High",
                _ => severity
            };
        }

        [HttpGet("test-urgency")]
        public IActionResult TestUrgencyCalculation()
        {
            try
            {
                // Using the Task D Incident class (the one with urgency calculation)
                var recentHigh = new Models.Incident("Server Outage", "High", DateTime.Now.AddHours(-2));
                var oldMedium = new Models.Incident("Performance Issue", "Medium", DateTime.Now.AddDays(-7));
                var oldLow = new Models.Incident("UI Bug", "Low", DateTime.Now.AddDays(-30));

                var results = new[]
                {
            new {
                Title = recentHigh.Title,
                Severity = recentHigh.Severity,
                AgeDays = recentHigh.AgeDays,
                Urgency = recentHigh.CalculateUrgency(),
                Description = recentHigh.GetUrgencyDescription(),
                IsUrgent = recentHigh.IsUrgent()
            },
            new {
                Title = oldMedium.Title,
                Severity = oldMedium.Severity,
                AgeDays = oldMedium.AgeDays,
                Urgency = oldMedium.CalculateUrgency(),
                Description = oldMedium.GetUrgencyDescription(),
                IsUrgent = oldMedium.IsUrgent()
            },
            new {
                Title = oldLow.Title,
                Severity = oldLow.Severity,
                AgeDays = oldLow.AgeDays,
                Urgency = oldLow.CalculateUrgency(),
                Description = oldLow.GetUrgencyDescription(),
                IsUrgent = oldLow.IsUrgent()
            }
        };

                return Ok(new
                {
                    message = "Task D: Incident Class with Urgency Calculation",
                    businessLogic = "Urgency increases based on severity and age. High=8 base, Medium=5 base, Low=2 base. Age multipliers apply.",
                    testResults = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

    }

    // Data models
    public class CreateIncidentRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Severity is required")]
        public string Severity { get; set; } = string.Empty;
    }

    public class Incident
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class IncidentResponse
    {
        public IncidentResponse(Incident incident)
        {
            Id = incident.Id;
            Title = incident.Title;
            Description = incident.Description;
            Severity = incident.Severity;
            CreatedAt = incident.CreatedAt;
            Status = incident.Status;
        }

        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}