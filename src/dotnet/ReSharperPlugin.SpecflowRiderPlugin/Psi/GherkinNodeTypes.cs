using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
// ReSharper disable InconsistentNaming

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public static class GherkinNodeTypes
    {
        private static int _lastNodeId = 3000;
        private static int NextId => ++_lastNodeId;
        
        public static readonly GherkinNodeType FILE = new GherkinFileNodeType("FILE", NextId);
        public static readonly GherkinNodeType TAG = new GherkinTagNodeType("TAG", NextId);
        public static readonly GherkinNodeType FEATURE_HEADER = new GherkinFeatureHeaderNodeType("FEATURE_HEADER", NextId);
        public static readonly GherkinNodeType FEATURE = new GherkinFeatureNodeType("FEATURE", NextId);

        private class GherkinFileNodeType : GherkinNodeType
        {
            public GherkinFileNodeType(string name, int index) : base(name, index)
            {
            }

            public override CompositeElement Create(object userData)
            {
                return new GherkinFile((string) userData);
            }
        }
        
        private class GherkinTagNodeType : GherkinNodeType<GherkinTag>
        {
            public GherkinTagNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinFeatureNodeType : GherkinNodeType<GherkinFeature>
        {
            public GherkinFeatureNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinFeatureHeaderNodeType : GherkinNodeType<GherkinFeatureHeader>
        {
            public GherkinFeatureHeaderNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
    }
}