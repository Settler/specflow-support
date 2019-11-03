namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinScenario : GherkinElement
    {
        public GherkinScenario() : base(GherkinNodeTypes.SCENARIO)
        {
        }
        
        public override string ToString()
        {
            var featureNameToken = FindDescendant<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinScenario: {featureNameToken?.GetText()}";
        }
    }
}