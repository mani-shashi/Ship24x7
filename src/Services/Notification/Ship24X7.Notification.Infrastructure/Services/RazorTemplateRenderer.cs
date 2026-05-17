using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Ship24X7.Notification.Application.Interfaces;

namespace Ship24X7.Notification.Infrastructure.Services;

/// <summary>
/// Implements template rendering by replacing {{placeholder}} markers with actual values from data dictionary.
/// Uses simple string replacement algorithm (not full Razor engine) for performance and simplicity.
/// Processes templates with double curly brace syntax {{PlaceholderName}} and replaces with corresponding dictionary values.
/// Used by SendNotificationCommandHandler to render notification subject and body before delivery.
/// Validates that all placeholders are replaced and logs warnings for unreplaced placeholders indicating missing data.
/// </summary>
public class RazorTemplateRenderer : ITemplateRenderer
{
    private readonly ILogger<RazorTemplateRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorTemplateRenderer"/> class.
    /// Sets up logger for recording template rendering operations and warnings about unreplaced placeholders.
    /// </summary>
    /// <param name="logger">Logger for recording rendering operations, warnings about missing placeholders, and errors.</param>
    public RazorTemplateRenderer(ILogger<RazorTemplateRenderer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Renders a template by replacing all {{placeholder}} markers with actual values from the data dictionary.
    /// Logic flow: 1) Starts with original template string, 2) Iterates through placeholder data dictionary,
    /// 3) For each key-value pair, constructs placeholder pattern {{Key}} and replaces all occurrences with Value,
    /// 4) After all replacements, uses regex to find any remaining unreplaced placeholders {{...}},
    /// 5) Logs warning if unreplaced placeholders found (indicates missing data in dictionary),
    /// 6) Returns fully rendered string with all placeholders replaced.
    /// Uses case-sensitive matching - placeholder names must match dictionary keys exactly.
    /// </summary>
    /// <param name="template">Template string containing text and {{PlaceholderName}} markers. Can contain HTML tags for email templates.</param>
    /// <param name="placeholderData">Dictionary mapping placeholder names (without braces) to replacement values. Keys are case-sensitive.</param>
    /// <param name="cancellationToken">Cancellation token to abort rendering operation if request is cancelled.</param>
    /// <returns>
    /// Rendered string with all {{placeholders}} replaced by corresponding dictionary values.
    /// Unreplaced placeholders remain as {{PlaceholderName}} if key not found in dictionary (logged as warning).
    /// </returns>
    /// <exception cref="Exception">Rethrows any exception that occurs during rendering after logging error.</exception>
    public Task<string> RenderAsync(string template, Dictionary<string, string> placeholderData, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = template;

            // Replace placeholders in the format {{PlaceholderName}} with actual values
            foreach (var kvp in placeholderData)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}"; // Construct {{Key}} pattern
                result = result.Replace(placeholder, kvp.Value); // Replace all occurrences
            }

            // Check for any remaining unreplaced placeholders using regex pattern
            var unreplacedPlaceholders = Regex.Matches(result, @"\{\{([^}]+)\}\}");
            if (unreplacedPlaceholders.Count > 0)
            {
                // Log warning if placeholders remain - indicates missing data in dictionary
                _logger.LogWarning("Template contains unreplaced placeholders: {Placeholders}", 
                    string.Join(", ", unreplacedPlaceholders.Select(m => m.Value)));
            }

            return Task.FromResult(result); // Return rendered template
        }
        catch (Exception ex)
        {
            // Log error and rethrow - caller will handle by marking notification as failed
            _logger.LogError(ex, "Error rendering template");
            throw;
        }
    }
}
