using QuestMail.Core.Components;
using QuestMail.Core.Internal;
using QuestMail.Core.Styles;

namespace QuestMail.Core.Fluent;

public sealed class RowBuilder : ContainerBuilder<RowBuilder, Row>
{
    private double _gap;
    private int _mobileColumns = 1;

    public RowBuilder WithGap(double gap)
    {
        Guard.NonNegative(gap, nameof(gap));
        _gap = gap;
        return this;
    }

    public RowBuilder WithMobileColumns(int columns)
    {
        if (columns < 1)
            throw new ArgumentOutOfRangeException(nameof(columns));
        _mobileColumns = columns;
        return this;
    }

    private protected override Row Create(IEnumerable<EmailComponent> children,
        ComponentStyle style, ComponentStyle? mobileStyle) => new(children)
        {
            Style = style, MobileStyle = mobileStyle, Gap = _gap, MobileColumns = _mobileColumns
        };
}
