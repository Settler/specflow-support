namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinStep : GherkinElement
    {
        public GherkinStep() : base(GherkinNodeTypes.STEP)
        {
        }
        
        public override string ToString()
        {
            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinStep: {textToken?.GetText()}";
        }
    }
}