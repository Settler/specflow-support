using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinParser : IParser
    {
        // ReSharper disable once InconsistentNaming
        private static readonly NodeTypeSet SCENARIO_END_TOKENS = new NodeTypeSet(
            GherkinTokenTypes.BACKGROUND_KEYWORD,
            GherkinTokenTypes.SCENARIO_KEYWORD,
            GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD,
            GherkinTokenTypes.RULE_KEYWORD,
            GherkinTokenTypes.FEATURE_KEYWORD);

        private readonly IPsiSourceFile _sourceFile;
        private readonly PsiBuilder _builder;

        public GherkinParser(ILexer lexer, IPsiSourceFile sourceFile)
        {
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
                    ParseTags(_builder);
                else
                    _builder.AdvanceLexer();
            }

            _builder.Done(fileMarker, GherkinNodeTypes.FILE, _sourceFile.Name);
            var resultTree = (GherkinFile) _builder.BuildTree();

            return resultTree;
        }

        private static void ParseTags(PsiBuilder builder)
        {
            while (builder.GetTokenType() == GherkinTokenTypes.TAG)
            {
                var tagMarker = builder.Mark();
                builder.AdvanceLexer();
                builder.Done(tagMarker, GherkinNodeTypes.TAG, null);
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

        private static void ParseFeatureElements(PsiBuilder builder)
        {
//            PsiBuilder.Marker ruleMarker = null;
            while (builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !builder.Eof())
            {
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
                var marker = builder.Mark();
                // tags
                ParseTags(builder);

                // scenarios
                var startTokenType = builder.GetTokenType();
                var outline = startTokenType == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD;
                builder.AdvanceLexer();
                ParseScenario(builder);
                builder.Done(marker, outline ? GherkinNodeTypes.SCENARIO_OUTLINE : GherkinNodeTypes.SCENARIO, null);
            }

//            if (ruleMarker != null) {
//                ruleMarker.done(RULE);
//            }
        }

        private static void ParseScenario(PsiBuilder builder)
        {
            while (!AtScenarioEnd(builder))
            {
                if (builder.GetTokenType() == GherkinTokenTypes.TAG)
                {
                    var marker = builder.Mark();
                    ParseTags(builder);
                    if (AtScenarioEnd(builder))
                    {
                        builder.RollbackTo(marker);
                        break;
                    }

                    builder.Drop(marker);
                }

                if (ParseStepParameter(builder))
                    continue;

                if (builder.GetTokenType() == GherkinTokenTypes.STEP_KEYWORD)
                    ParseStep(builder);
                else if (builder.GetTokenType() == GherkinTokenTypes.EXAMPLES_KEYWORD)
                    ParseExamplesBlock(builder);
                else
                    builder.AdvanceLexer();
            }
        }

        private static void ParseStep(PsiBuilder builder)
        {
            var marker = builder.Mark();
            builder.AdvanceLexer();
            var prevTokenEnd = -1;
            while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.STEP_PARAMETER_BRACE ||
                   builder.GetTokenType() == GherkinTokenTypes.STEP_PARAMETER_TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
            {
                var tokenText = builder.GetTokenText();
                if (HadLineBreakBefore(builder, prevTokenEnd))
                    break;

                prevTokenEnd = builder.GetTokenOffset() + GetTokenLength(tokenText);
                if (!ParseStepParameter(builder))
                    builder.AdvanceLexer();
            }

            var tokenTypeAfterName = builder.GetTokenType();
            if (tokenTypeAfterName == GherkinTokenTypes.PIPE)
                ParseTable(builder);
            else if (tokenTypeAfterName == GherkinTokenTypes.PYSTRING)
                ParsePystring(builder);

            builder.Done(marker, GherkinNodeTypes.STEP, null);
        }

        private static void ParseExamplesBlock(PsiBuilder builder)
        {
            var marker = builder.Mark();
            builder.AdvanceLexer();
            if (builder.GetTokenType() == GherkinTokenTypes.COLON)
                builder.AdvanceLexer();

            while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
                builder.AdvanceLexer();

            if (builder.GetTokenType() == GherkinTokenTypes.PIPE)
                ParseTable(builder);

            builder.Done(marker, GherkinNodeTypes.EXAMPLES_BLOCK, null);
        }

        private static void ParseTable(PsiBuilder builder)
        {
            var marker = builder.Mark();
            var rowMarker = builder.Mark();
            var headerNodeType = GherkinNodeTypes.TABLE_HEADER_ROW;
            int? cellMarker = null;

            var wasLineBreak = false;
            var possibleEmptyCell = false;
            while (builder.GetTokenType() == GherkinTokenTypes.PIPE ||
                   builder.GetTokenType() == GherkinTokenTypes.TABLE_CELL ||
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
            {
                var tokenType = builder.GetTokenType();
                if (tokenType == GherkinTokenTypes.TABLE_CELL && cellMarker == null)
                {
                    cellMarker = builder.Mark();
                }
                else if (tokenType != GherkinTokenTypes.TABLE_CELL && cellMarker != null)
                {
                    builder.Done(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);
                    cellMarker = null;
                    possibleEmptyCell = false;
                }
                
                if (tokenType == GherkinTokenTypes.PIPE)
                {
                    if (wasLineBreak)
                    {
                        possibleEmptyCell = true;
                        builder.Done(rowMarker, headerNodeType, null);
                        headerNodeType = GherkinNodeTypes.TABLE_ROW;
                        rowMarker = builder.Mark();
                    }
                    else
                    {
                        if (possibleEmptyCell)
                        {
                            cellMarker = builder.Mark();
                            builder.Done(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);
                            cellMarker = null;
                        }

                        possibleEmptyCell = true;
                    }
                }

                wasLineBreak = IsLineBreak(builder);
                builder.AdvanceLexer();
            }

            if (cellMarker.HasValue)
                builder.Done(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);

            builder.Done(rowMarker, headerNodeType, null);
            builder.Done(marker, GherkinNodeTypes.TABLE, null);
        }

        private static void ParsePystring(PsiBuilder builder)
        {
            if (!builder.Eof())
            {
                var marker = builder.Mark();
                builder.AdvanceLexer();
                while (!builder.Eof() && builder.GetTokenType() != GherkinTokenTypes.PYSTRING)
                {
                    if (!ParseStepParameter(builder))
                        builder.AdvanceLexer();
                }

                if (!builder.Eof())
                    builder.AdvanceLexer();

                builder.Done(marker, GherkinNodeTypes.PYSTRING, null);
            }
        }

        private static bool ParseStepParameter(PsiBuilder builder)
        {
            if (builder.GetTokenType() != GherkinTokenTypes.STEP_PARAMETER_TEXT)
                return false;

            var stepParameterMarker = builder.Mark();
            builder.AdvanceLexer();
            builder.Done(stepParameterMarker, GherkinNodeTypes.STEP_PARAMETER, null);
            return true;
        }

        private static bool AtScenarioEnd(PsiBuilder builder)
        {
            var i = 0;
            while (builder.GetTokenType(i) == GherkinTokenTypes.TAG)
                i++;

            var tokenType = builder.GetTokenType(i);
            return tokenType == null || SCENARIO_END_TOKENS[tokenType];
        }

        private static int GetTokenLength(string tokenText)
        {
            return tokenText?.Length ?? 0;
        }

        private static bool HadLineBreakBefore(PsiBuilder builder, int prevTokenEnd)
        {
            if (prevTokenEnd < 0)
                return false;

            var possibleLineBreakRange = new TextRange(prevTokenEnd, builder.GetTokenOffset());
            var possibleLineBreakText = builder.GetBuffer().GetText(possibleLineBreakRange);
            var lineBreakIndex = possibleLineBreakText.IndexOf('\n');
            if (lineBreakIndex != -1)
                return true;

            return false;
        }

        private static bool IsLineBreak(PsiBuilder builder)
        {
            return builder.GetTokenText()?.Contains("\n") == true;
        }
    }
}