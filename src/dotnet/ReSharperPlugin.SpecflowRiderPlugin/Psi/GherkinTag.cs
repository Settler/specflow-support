using JetBrains.Diagnostics;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTag : GherkinElement
    {
        public GherkinTag() : base(GherkinNodeTypes.TAG)
        {
        }
    }
}