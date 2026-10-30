using QuestMail.Core.Styles;

namespace QuestMail.Core.Tests;

public class CssDeclarationsTests
{
    [Fact]
    public void CreateNormalizesNamesAndLastDuplicateWinsInCascadeOrder()
    {
        var css = CssDeclarations.Create((" COLOR ", " red "), ("font-size", "18px"), ("Color", "blue"));

        Assert.Equal(2, css.Count);
        Assert.Equal("blue", css["COLOR"]);
        Assert.Equal(new[] { "font-size", "color" }, css.Keys);
        Assert.Equal(new[] { "18px", "blue" }, css.Values);
        Assert.Equal("font-size:18px;color:blue;", css.ToString());
        Assert.True(css.ContainsKey("Color"));
        Assert.False(css.TryGetValue("missing", out _));
        Assert.Throws<KeyNotFoundException>(() => css["missing"]);
    }

    [Fact]
    public void CollectionsSnapshotInputAndSetAndRemoveNeverMutateTheOriginal()
    {
        var input = new[] { ("color", "red") };
        var css = CssDeclarations.Create(input);
        input[0] = ("color", "blue");
        var changed = css.Set("text-transform", "uppercase").Set("color", "green");
        var removed = changed.Remove("COLOR");
        var dictionary = new Dictionary<string, string> { ["color"] = "purple" };
        var fromDictionary = CssDeclarations.Create(dictionary);
        dictionary["color"] = "orange";

        Assert.Empty(CssDeclarations.Empty);
        Assert.Equal("red", css["color"]);
        Assert.Equal("text-transform:uppercase;color:green;", changed.ToString());
        Assert.Equal("text-transform:uppercase;", removed.ToString());
        Assert.Equal("purple", fromDictionary["color"]);
        Assert.False((object)css is IDictionary<string, string>);
    }

    [Fact]
    public void EqualityAndHashingIncludeDeclarationOrder()
    {
        var first = CssDeclarations.Create(("color", "red"), ("font-size", "16px"));
        var second = CssDeclarations.Create(("COLOR", "red"), ("font-size", "16px"));
        var reversed = CssDeclarations.Create(("font-size", "16px"), ("color", "red"));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.NotEqual(first, reversed);
        Assert.Equal(new ComponentStyle { CustomCss = first }, new ComponentStyle { CustomCss = second });
        Assert.Equal(CssDeclarations.Empty, CssDeclarations.Create());
    }

    [Theory]
    [InlineData("display")]
    [InlineData("position")]
    [InlineData("width")]
    [InlineData("height")]
    [InlineData("padding")]
    [InlineData("margin")]
    [InlineData("table-layout")]
    [InlineData("visibility")]
    [InlineData("all")]
    [InlineData("background-image")]
    [InlineData("--custom-property")]
    [InlineData("-moz-binding")]
    [InlineData("behavior")]
    [InlineData("@import")]
    [InlineData("color;display")]
    [InlineData("color\n")]
    [InlineData("")]
    public void UnknownStructuralAndUnsafePropertiesAreRejected(string property) =>
        Assert.Throws<ArgumentException>(() => CssDeclarations.Create((property, "red")));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("red;color:blue")]
    [InlineData("red!important")]
    [InlineData("red}body{color:blue")]
    [InlineData("</style><script>alert(1)</script>")]
    [InlineData("red\r\n")]
    [InlineData("red\t")]
    [InlineData("url(https://example.com/image)")]
    [InlineData("expression(alert(1))")]
    [InlineData("var(--color)")]
    [InlineData("rgb(10, 20, 30)")]
    [InlineData("red/*comment*/")]
    [InlineData("r\\65 d")]
    [InlineData("'unclosed font")]
    [InlineData("red\0")]
    public void UnsafeOrUnsupportedValueSyntaxIsRejected(string value) =>
        Assert.Throws<ArgumentException>(() => CssDeclarations.Create(("color", value)));

    [Fact]
    public void NullAndOversizedInputsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => CssDeclarations.Create((ValueTuple<string, string>[])null!));
        Assert.Throws<ArgumentNullException>(() => CssDeclarations.Create((IEnumerable<KeyValuePair<string, string>>)null!));
        Assert.Throws<ArgumentNullException>(() => CssDeclarations.Empty.Set(null!, "red"));
        Assert.Throws<ArgumentNullException>(() => CssDeclarations.Empty.Set("color", null!));
        Assert.Throws<ArgumentException>(() => CssDeclarations.Empty.Set("color", new string('a', 513)));
    }

    [Theory]
    [InlineData("text-transform", "uppercase")]
    [InlineData("word-spacing", "2px")]
    [InlineData("letter-spacing", "-0.5px")]
    [InlineData("line-height", "1.5")]
    [InlineData("font-family", "\"Times New Roman\", 'Segoe UI', serif")]
    [InlineData("color", "#334155")]
    [InlineData("opacity", "0.8")]
    [InlineData("text-shadow", "1px 2px 3px #000000")]
    public void SimpleVisualValuesAreSupported(string property, string value) =>
        Assert.Equal(value, CssDeclarations.Empty.Set(property, value)[property]);
}
