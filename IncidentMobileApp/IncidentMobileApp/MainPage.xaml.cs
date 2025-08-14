using System.Text;
using System.Text.Json;

namespace IncidentMobileApp;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://10.0.2.2:7097";

    public MainPage()
    {
        InitializeComponent();

        // Configure HttpClient for development (ignore SSL certificates)
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        await SubmitIncident();
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void OnReportAnotherClicked(object sender, EventArgs e)
    {
        // Hide success screen and show form again
        SuccessAnimationFrame.IsVisible = false;
        MainFormContainer.IsVisible = true;
        ClearForm();
    }

    private void OnDismissErrorClicked(object sender, EventArgs e)
    {
        HideError();
    }

    private async Task SubmitIncident()
    {
        try
        {
            // Validate form
            if (!ValidateForm())
            {
                ShowError("Please fill in all required fields:\n• Incident Title\n• Description\n• Severity Level");
                return;
            }

            // Show elegant loading screen
            ShowLoading(true);
            HideError();

            // Transform mobile field names to API field names
            var mobileFormData = new MobileIncidentForm
            {
                incident_title = TitleEntry.Text?.Trim(),
                incident_description = DescriptionEditor.Text?.Trim(),
                incident_severity = SeverityPicker.SelectedItem?.ToString()
            };

            // Map to API format
            var apiPayload = TransformToApiFormat(mobileFormData);

            // Submit to API
            var json = JsonSerializer.Serialize(apiPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/incidents", content);

            if (response.IsSuccessStatusCode)
            {
                // Parse response to get incident details
                var responseContent = await response.Content.ReadAsStringAsync();
                var incidentResponse = JsonSerializer.Deserialize<IncidentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Show beautiful success animation
                ShowSuccess(mobileFormData.incident_title ?? "Unnamed Incident", incidentResponse?.Id);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ShowError($"Failed to submit incident.\n\nServer Response: {response.StatusCode}\n{errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            ShowError($"Network connection failed.\n\nPlease check:\n• Internet connection\n• API server is running\n• Firewall settings\n\nError: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            ShowError("Request timed out.\n\nThe server took too long to respond.\nPlease try again.");
        }
        catch (Exception ex)
        {
            ShowError($"An unexpected error occurred:\n\n{ex.Message}\n\nPlease try again or contact support.");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private bool ValidateForm()
    {
        return !string.IsNullOrWhiteSpace(TitleEntry.Text) &&
               !string.IsNullOrWhiteSpace(DescriptionEditor.Text) &&
               SeverityPicker.SelectedItem != null;
    }

    private ApiIncidentRequest TransformToApiFormat(MobileIncidentForm mobileForm)
    {
        // Field name transformation: mobile form → API format
        return new ApiIncidentRequest
        {
            Title = mobileForm.incident_title,           // incident_title → Title
            Description = mobileForm.incident_description, // incident_description → Description  
            Severity = mobileForm.incident_severity      // incident_severity → Severity
        };
    }

    private void ClearForm()
    {
        TitleEntry.Text = string.Empty;
        DescriptionEditor.Text = string.Empty;
        SeverityPicker.SelectedItem = null;
        HideError();
    }

    private void ShowSuccess(string incidentTitle, Guid? incidentId)
    {
        // Create personalized success message
        var shortId = incidentId?.ToString()[..8] ?? "Unknown";
        SuccessMessageLabel.Text = $"Incident '{incidentTitle}' has been successfully submitted!\n\nIncident ID: {shortId}...";

        // Hide main form and show success animation
        MainFormContainer.IsVisible = false;
        SuccessAnimationFrame.IsVisible = true;

        // Scroll to top to show success message
        var scrollView = this.FindByName<ScrollView>("ScrollView");
        scrollView?.ScrollToAsync(0, 0, true);
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorFrame.IsVisible = true;

        // Scroll to error message
        var scrollView = this.FindByName<ScrollView>("ScrollView");
        scrollView?.ScrollToAsync(ErrorFrame, ScrollToPosition.MakeVisible, true);
    }

    private void HideError()
    {
        ErrorFrame.IsVisible = false;
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingFrame.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;

        // Disable form interaction while loading
        SubmitBtn.IsEnabled = !isLoading;
        ClearBtn.IsEnabled = !isLoading;
        TitleEntry.IsEnabled = !isLoading;
        DescriptionEditor.IsEnabled = !isLoading;
        SeverityPicker.IsEnabled = !isLoading;

        if (isLoading)
        {
            HideError();
            // Scroll to loading indicator
            var scrollView = this.FindByName<ScrollView>("ScrollView");
            scrollView?.ScrollToAsync(LoadingFrame, ScrollToPosition.MakeVisible, true);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _httpClient?.Dispose();
    }
}

// Data models for field transformation
public class MobileIncidentForm
{
    public string? incident_title { get; set; }        // Mobile form field name
    public string? incident_description { get; set; }  // Mobile form field name  
    public string? incident_severity { get; set; }     // Mobile form field name
}

public class ApiIncidentRequest
{
    public string? Title { get; set; }        // API expected field name
    public string? Description { get; set; }  // API expected field name
    public string? Severity { get; set; }     // API expected field name
}

public class IncidentResponse
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Status { get; set; }
}
