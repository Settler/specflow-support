namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinStepParameter : GherkinElement
    {
        public GherkinStepParameter() : base(GherkinNodeTypes.STEP_PARAMETER)
        {
        }
        
        public override string ToString()
        {
            var textToken = this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.STEP_PARAMETER_TEXT);
            return $"GherkinStepParameter: {textToken?.GetText()}";
        }
    }
}