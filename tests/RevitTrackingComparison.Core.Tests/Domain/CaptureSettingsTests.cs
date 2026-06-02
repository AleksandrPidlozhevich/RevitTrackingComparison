using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Tests.Domain;

[TestFixture]
public sealed class CaptureSettingsTests
{
    private static CaptureSettings CreateSettings(params CaptureRule[] rules) =>
        new() { Rules = rules };

    [Test]
    public void IncludesCategory_is_case_insensitive()
    {
        var settings = CreateSettings(new CaptureRule { Category = "Walls", Parameters = ["Height"] });

        Assert.That(settings.IncludesCategory("walls"), Is.True);
        Assert.That(settings.IncludesCategory("WALLS"), Is.True);
        Assert.That(settings.IncludesCategory("Doors"), Is.False);
    }

    [Test]
    public void ParametersFor_returns_parameters_for_known_category()
    {
        var settings = CreateSettings(
            new CaptureRule { Category = "Walls", Parameters = ["Height", "Width"] },
            new CaptureRule { Category = "Doors", Parameters = ["Mark"] });

        var parameters = settings.ParametersFor("walls");

        Assert.That(parameters, Is.EqualTo(new[] { "Height", "Width" }));
    }

    [Test]
    public void ParametersFor_returns_empty_for_unknown_category()
    {
        var settings = CreateSettings(new CaptureRule { Category = "Walls", Parameters = ["Height"] });

        Assert.That(settings.ParametersFor("Roofs"), Is.Empty);
    }
}
