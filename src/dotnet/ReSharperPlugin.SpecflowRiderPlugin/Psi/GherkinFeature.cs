using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinFeature : GherkinElement
    {
        public GherkinFeature() : base(GherkinNodeTypes.FEATURE)
        {
        }

        public override string ToString()
        {
            var featureNameToken = FindDescendant<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT);
            return $"GherkinFeature: {featureNameToken?.GetText()}";
        }
    }
}