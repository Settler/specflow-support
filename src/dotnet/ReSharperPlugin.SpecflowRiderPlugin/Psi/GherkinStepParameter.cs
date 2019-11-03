namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinStepParameter : GherkinElement
    {
        public GherkinStepParameter() : base(GherkinNodeTypes.STEP_PARAMETER)
        {
        }
        
        public override string ToString()
        {
            var featureNameToken = FindDescendant<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinStepParameter: {featureNameToken?.GetText()}";
        }
    }
}