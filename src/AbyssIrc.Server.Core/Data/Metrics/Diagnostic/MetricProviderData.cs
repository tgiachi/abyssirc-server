namespace AbyssIrc.Server.Core.Data.Metrics.Diagnostic;

public class MetricProviderData
{
    public string Name { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
        return Value.ToString();
    }

    public MetricProviderData(string name, object value)
    {
        Name = name;
        Value = value;
    }

    public MetricProviderData()
    {

    }
}
