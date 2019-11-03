using System.Text;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Text;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinParser : IParser
    {
        private static NodeTypeSet SCENARIO_END_TOKENS = new NodeTypeSet(
            GherkinTokenTypes.BACKGROUND_KEYWORD,
            GherkinTokenTypes.SCENARIO_KEYWORD,
            GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD,
            GherkinTokenTypes.RULE_KEYWORD,
            GherkinTokenTypes.FEATURE_KEYWORD);
        
        private readonly IPsiModule _module;
        private readonly IPsiSourceFile _sourceFile;
        private readonly PsiBuilder _builder;

        public GherkinParser(ILexer lexer, IPsiModule module, IPsiSourceFile sourceFile)
        {
            _module = module;
            _sourceFile = sourceFile;
            _builder = new PsiBuilder(lexer, GherkinNodeTypes.FILE, null, Lifetime.Eternal);
        }

        public IFile ParseFile()
        {
            var fileMarker = _builder.Mark();
            
            while (!_builder.Eof())
            {
                var tokenType = _builder.GetTokenType();
                
                if (tokenType == GherkinTokenTypes.FEATURE_KEYWORD)
                    ParseFeature();
                else if (tokenType == GherkinTokenTypes.TAG)
                    ParseTags();
                else
                    _builder.AdvanceLexer();
            }

            _builder.Done(fileMarker, GherkinNodeTypes.FILE, _sourceFile.Name);
            var resultTree = (GherkinFile) _builder.BuildTree();

            return resultTree;
        }

        private void ParseTags()
        {
            while (_builder.GetTokenType() == GherkinTokenTypes.TAG)
            {
                var tagMarker = _builder.Mark();
                _builder.AdvanceLexer();
                _builder.Done(tagMarker, GherkinNodeTypes.TAG, null);
            }
        }

        private void ParseFeature()
        {
            var featureMarker = _builder.Mark();

            Assertion.Assert(_builder.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD,
                "_builder.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD");

            int featureEnd = _builder.GetTokenOffset() + GetTokenLength(_builder.GetTokenText());

            int? descMarker = null;
            do
            {
                _builder.AdvanceLexer();

                var tokenType = _builder.GetTokenType();
                if (tokenType == GherkinTokenTypes.TEXT && descMarker == null)
                {
                    if (HadLineBreakBefore(_builder, featureEnd))
                        descMarker = _builder.Mark();
                }

                if (GherkinTokenTypes.SCENARIOS_KEYWORDS[tokenType] ||
                    tokenType == GherkinTokenTypes.RULE_KEYWORD ||
                    tokenType == GherkinTokenTypes.BACKGROUND_KEYWORD ||
                    tokenType == GherkinTokenTypes.TAG)
                {
                    if (descMarker != null)
                    {
                        _builder.Done(descMarker.Value, GherkinNodeTypes.FEATURE_HEADER, null);
                        descMarker = null;
                    }

                    ParseFeatureElements(_builder);
                }
            } while (_builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !_builder.Eof());

            if (descMarker != null)
                _builder.Done(descMarker.Value, GherkinNodeTypes.FEATURE_HEADER, null);

            _builder.Done(featureMarker, GherkinNodeTypes.FEATURE, null);
        }
        
        private static void ParseFeatureElements(PsiBuilder builder) {
//            PsiBuilder.Marker ruleMarker = null;
            while (builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !builder.Eof()) {
//                if (builder.getTokenType() == RULE_KEYWORD) {
//                    if (ruleMarker != null) {
//                        ruleMarker.done(RULE);
//                    }
//                    ruleMarker = builder.mark();
//                    builder.advanceLexer();
//                    if (builder.getTokenType() == COLON) {
//                        builder.advanceLexer();
//                    } else {
//                        break;
//                    }
//
//                    while (builder.getTokenType() == TEXT) {
//                        builder.advanceLexer();
//                    }
//                }
//
//                final PsiBuilder.Marker marker = builder.mark();
//                // tags
//                parseTags(builder);

                // scenarios
//                IElementType startTokenType = builder.getTokenType();
//                final boolean outline = startTokenType == SCENARIO_OUTLINE_KEYWORD;
                builder.AdvanceLexer();
//                parseScenario(builder);
//                marker.done(outline ? GherkinElementTypes.SCENARIO_OUTLINE : GherkinElementTypes.SCENARIO);
            }
//            if (ruleMarker != null) {
//                ruleMarker.done(RULE);
//            }
        }

        private static int GetTokenLength(string tokenText) {
            return tokenText?.Length ?? 0;
        }
        
        private static bool HadLineBreakBefore(PsiBuilder builder, int prevTokenEnd) {
            if (prevTokenEnd < 0)
                return false;

            var possibleLineBreakRange = new TextRange(prevTokenEnd, builder.GetTokenOffset());
            var possibleLineBreakText = builder.GetBuffer().GetText(possibleLineBreakRange);
            var lineBreakIndex = possibleLineBreakText.IndexOf('\n');
            if (lineBreakIndex != -1)
                return true;
            
            return false;
        }
    }
}