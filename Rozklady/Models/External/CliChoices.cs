public class CliChoices
{
    public Customer Provider { get; set; } = default!;
    public string ProxyOption { get; set; } = "none"; // "none" | "custom"
    public List<string> CustomProxies { get; set; } = new();
}

