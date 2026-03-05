// =============================================================================
// UserEvent.cs - Sample domain model representing user activity/behavior events
// =============================================================================
//
// PURPOSE:
//   UserEvents represent clickstream/behavioral data - one of the most common
//   use cases for Apache Kafka. Think of how Netflix, LinkedIn, or any web app
//   tracks what users do in real-time.
//
// REAL-WORLD CONTEXT:
//   User activity events are published at very HIGH VOLUME (potentially millions
//   per second across all users). Kafka excels at this pattern because:
//   - It can handle millions of messages per second
//   - Events are retained for replay (e.g., reprocess for ML training)
//   - Multiple consumers can process the same events independently
//     (recommendations engine, analytics, fraud detection, etc.)
//
// KAFKA RELEVANCE:
//   - UserId as message key ensures events for a user go to the same partition,
//     preserving the sequence of a user's actions (important for session analysis)
//   - High-volume nature benefits from Kafka's compression and batching
//   - The 'SessionId' enables stream processing to group events into sessions

namespace Kafka101.Models;

/// <summary>
/// Represents a user activity event captured from a web or mobile application.
/// This is a classic "clickstream" event used for analytics, recommendations,
/// and real-time personalization.
/// </summary>
public class UserEvent
{
    // -------------------------------------------------------------------------
    // KAFKA KEY STRATEGY:
    // For user events, you have two main key choices:
    // 1. UserId - ensures all events for a user go to the same partition,
    //    preserving action sequence. Good for per-user analytics.
    // 2. SessionId - groups events by browsing session. Good for session analysis.
    // 3. null key - round-robin distribution, best throughput but no ordering.
    //
    // Choose based on what ordering guarantee matters most to your consumers.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique identifier for the user. Used as Kafka message key for ordering.
    /// For anonymous users, use a device fingerprint or session cookie value.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Session identifier groups events from a single browsing session.
    /// Useful for funnel analysis and user journey mapping.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The type of action the user performed.
    /// Examples: "PageView", "ProductClick", "AddToCart", "Purchase", "Search"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The page or screen where the event occurred.
    /// </summary>
    public string Page { get; set; } = string.Empty;

    /// <summary>
    /// Additional context-specific properties as key-value pairs.
    /// Using a dictionary allows flexible schema without changing the model.
    /// Example: {"productId": "PROD-123", "searchQuery": "laptop", "clickPosition": "3"}
    ///
    /// NOTE: In production, consider using a more structured approach like
    /// Avro schemas to avoid schema drift and enable proper validation.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// When the event occurred on the CLIENT side (not server-receive time).
    /// Client-side timestamps can be skewed; stream processors often use
    /// "event time" vs "processing time" to handle late-arriving events.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Platform where the event occurred. Helps segment analytics.
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Geographic region (country or city). Useful for geo-specific recommendations.
    /// Privacy note: In production, ensure GDPR/privacy compliance before storing.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // STREAM PROCESSING NOTE:
    // When processing these events in a stream processor (e.g., Kafka Streams),
    // you'd group by SessionId and count events to detect:
    // - Session duration (time between first and last event)
    // - Bounce rate (sessions with only 1 page view)
    // - Conversion funnel (% of sessions reaching Purchase event)
    // -------------------------------------------------------------------------

    public override string ToString() =>
        $"[{EventType}] User: {UserId} | Page: {Page} | " +
        $"Session: {SessionId} | Platform: {Platform} | " +
        $"Time: {Timestamp:HH:mm:ss.fff} UTC";
}

/// <summary>
/// Factory for generating realistic user event test data.
/// </summary>
public static class UserEventFactory
{
    private static readonly Random _random = new();

    private static readonly string[] _userIds =
        ["USER-AAA", "USER-BBB", "USER-CCC", "USER-DDD", "USER-EEE"];

    private static readonly string[] _eventTypes =
        ["PageView", "ProductClick", "AddToCart", "RemoveFromCart",
         "Checkout", "Purchase", "Search", "WishlistAdd", "ReviewSubmit"];

    private static readonly string[] _pages =
        ["/home", "/products", "/product/detail", "/cart", "/checkout",
         "/order-confirmation", "/search", "/profile", "/wishlist"];

    private static readonly string[] _platforms = ["web", "ios", "android"];

    private static readonly string[] _regions =
        ["us-east", "us-west", "eu-central", "ap-southeast", "sa-east"];

    /// <summary>
    /// Creates a single realistic user event.
    /// </summary>
    public static UserEvent Create(string? userId = null, string? eventType = null)
    {
        var selectedEventType = eventType ?? _eventTypes[_random.Next(_eventTypes.Length)];

        var properties = new Dictionary<string, string>
        {
            ["referrer"] = "/home",
            ["viewDurationMs"] = _random.Next(500, 30000).ToString()
        };

        // Add event-specific properties to make data more realistic
        if (selectedEventType == "ProductClick" || selectedEventType == "AddToCart")
        {
            properties["productId"] = $"PROD-{_random.Next(1000, 9999)}";
            properties["productCategory"] = "Electronics";
            properties["price"] = (_random.Next(10, 500) + 0.99m).ToString();
        }
        else if (selectedEventType == "Search")
        {
            var queries = new[] { "laptop", "wireless mouse", "monitor", "keyboard", "webcam" };
            properties["searchQuery"] = queries[_random.Next(queries.Length)];
            properties["resultCount"] = _random.Next(0, 200).ToString();
        }
        else if (selectedEventType == "Purchase")
        {
            properties["orderId"] = $"ORD-{_random.Next(10000, 99999)}";
            properties["orderTotal"] = (_random.Next(20, 1000) + 0.99m).ToString();
        }

        return new UserEvent
        {
            UserId = userId ?? _userIds[_random.Next(_userIds.Length)],
            SessionId = $"SESS-{Guid.NewGuid():N}"[..12],
            EventType = selectedEventType,
            Page = _pages[_random.Next(_pages.Length)],
            Properties = properties,
            Timestamp = DateTime.UtcNow,
            Platform = _platforms[_random.Next(_platforms.Length)],
            Region = _regions[_random.Next(_regions.Length)]
        };
    }

    /// <summary>
    /// Simulates a user browsing session - a sequence of related events.
    /// Demonstrates event ordering within a Kafka partition (same key = same partition).
    /// </summary>
    public static IEnumerable<UserEvent> CreateSession(string? userId = null)
    {
        var uid = userId ?? _userIds[_random.Next(_userIds.Length)];
        var sessionId = $"SESS-{Guid.NewGuid():N}"[..12];
        var platform = _platforms[_random.Next(_platforms.Length)];

        // Typical e-commerce session: Browse -> Search -> View -> Cart -> Buy
        var sessionFlow = new[]
        {
            ("PageView", "/home"),
            ("Search", "/search"),
            ("PageView", "/products"),
            ("ProductClick", "/product/detail"),
            ("AddToCart", "/product/detail"),
            ("PageView", "/cart"),
            ("Checkout", "/checkout"),
            ("Purchase", "/order-confirmation")
        };

        foreach (var (eventType, page) in sessionFlow)
        {
            yield return new UserEvent
            {
                UserId = uid,
                SessionId = sessionId,
                EventType = eventType,
                Page = page,
                Platform = platform,
                Region = _regions[_random.Next(_regions.Length)],
                Timestamp = DateTime.UtcNow,
                Properties = new Dictionary<string, string>
                {
                    ["sessionStep"] = sessionFlow.ToList().FindIndex(s => s.Item1 == eventType).ToString()
                }
            };
        }
    }
}
