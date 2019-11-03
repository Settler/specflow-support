namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTableCell : GherkinElement
    {
        public GherkinTableCell() : base(GherkinNodeTypes.TABLE_CELL)
        {
        }
        
        public override string ToString()
        {
            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TABLE_CELL);
            return $"GherkinTableCell: {textToken?.GetText()}";
        }
    }
}