namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinExamplesBlock : GherkinElement
    {
        public GherkinExamplesBlock() : base(GherkinNodeTypes.EXAMPLES_BLOCK)
        {
        }
        
        public override string ToString()
        {
            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinExamplesBlock: {textToken?.GetText()}";
        }
    }
}