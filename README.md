# IncidentAPI

Full Stack .NET Developer Assessment
This repository contains solutions for all four technical challenges. Each task can be tested independently.

📋 Task Overview
TaskFocusTechnologyTest LocationAREST API DesignASP.NET Core Web APISwagger UIBUI ComponentRazor/MVCWeb BrowserCMobile App.NET MAUIAndroid/WindowsDClass DesignC# ObjectsAPI Endpoint

🔧 Prerequisites

Visual Studio 2022 (with .NET 8.0)
Android Emulator (for Task C mobile testing)


⚡ Testing Instructions
TASK A: REST API Design & Validation
🎯 Goal: Test incident API with duplicate detection and validation

Open IncidentAPI project in Visual Studio
Run the project (F5)
Navigate to Swagger UI: https://localhost:7097/swagger

✅ Test Cases:
json// 1. Valid submission
POST /api/incidents
{
  "title": "Server outage in production",
  "description": "Main web server not responding",
  "severity": "High"
}

// 2. Duplicate test (submit same request within 24 hours)
// Expected: 409 Conflict

// 3. Invalid severity
POST /api/incidents  
{
  "title": "Network issue",
  "description": "Connection problems",
  "severity": "Critical"  
}
// Expected: 400 Bad Request
📊 Expected Results:

✅ Valid incidents return 201 Created
❌ Duplicates return 409 Conflict
❌ Invalid severity returns 400 Bad Request


TASK B: Blazor/Razor Component
🎯 Goal: Test dynamic incident display with styling

Ensure IncidentAPI is running
Navigate to: https://localhost:7097/IncidentView

✅ What to Look For:

📊 Statistics cards showing incident counts
🔄 Dynamic table that updates with new data
🎨 Color-coded styling (High=red, Medium=yellow, Low=green)
⚡ Refresh button functionality
➕ Add Sample button creates new incidents

🎨 UI Features:

Color-coded severity badges
Hover effects on table rows
Responsive Bootstrap layout
Loading states and error handling


TASK C: Mobile Data Handling (.NET MAUI)
🎯 Goal: Test mobile form with API integration and field transformation
Setup:

Open IncidentMobileApp project
Ensure IncidentAPI is still running (Task A)
Set startup to Android Emulator or Windows Machine
Run the MAUI project (F5)

Testing:

Fill out the form:

Title: "Mobile Test Incident"
Description: "Testing mobile submission"
Severity: "Medium"


Click Submit
Verify success:

✅ Success animation appears
📱 Incident ID shown
🔄 "Report Another Incident" button works



Verify API Integration:

Go back to Swagger (https://localhost:7097/swagger)
GET /api/incidents
Confirm your mobile submission appears in the list

🔄 Field Transformation Verified:

Mobile form uses: incident_title, incident_description
API receives: Title, Description
Transformation handled automatically


TASK D: Class Design with Constraints
🎯 Goal: Test incident class with urgency calculation

Ensure IncidentAPI is running
Navigate to Swagger: https://localhost:7097/swagger
Find and execute: GET /api/incidents/test-urgency

📊 Expected Response:
json{
  "message": "Task D: Incident Class with Urgency Calculation",
  "businessLogic": "Urgency increases based on severity and age...",
  "testResults": [
    {
      "title": "Server Outage",
      "severity": "High", 
      "ageDays": 0,
      "urgency": 10,
      "description": "Critical - Immediate Action Required",
      "isUrgent": true
    }
  ]
}
✅ Validation Tests:

✅ High severity + recent = Urgency 8-10
✅ Medium severity + aged = Urgency 5-9
✅ Low severity + old = Urgency 2-4
✅ Descriptive urgency levels
✅ Age-based escalation logic
