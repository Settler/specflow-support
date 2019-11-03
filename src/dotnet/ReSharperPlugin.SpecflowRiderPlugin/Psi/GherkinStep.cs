namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinStep : GherkinElement
    {
        public GherkinStep() : base(GherkinNodeTypes.STEP)
        {
        }
        
        public override string ToString()
        {
            var featureNameToken = FindDescendant<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinStep: {featureNameToken?.GetText()}";
        }
    }
}