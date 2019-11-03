using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinKeywordProvider
    {
        public IReadOnlyCollection<string> getAllKeywords(string language)
        {
            return new[]
                   {
                       "Feature",
                       "Функционал",
                       "Предыстория",
                       "Background",
                       "Сценарий",
                       "Scenario",
                       "Структура сценария",
                       "Примеры",
                       "Examples",
                       "Допустим",
                       "Пусть",
                       "Дано",
                       "Given",
                       "Когда",
                       "Тогда",
                       "И",
                       "And",
                       "Также"
                   };
        }

        public bool isSpaceRequiredAfterKeyword(string myCurLanguage, string keyword)
        {
            return true;
        }

        public TokenNodeType getTokenType(string myCurLanguage, string keyword)
        {
            switch (keyword)
            {
                case "Функционал":
                case "Feature":
                    return GherkinTokenTypes.FEATURE_KEYWORD;
                case "Предыстория":
                case "Background":
                    return GherkinTokenTypes.BACKGROUND_KEYWORD;
                case "Сценарий":
                case "Scenario":
                    return GherkinTokenTypes.SCENARIO_KEYWORD;
                case "Структура сценария":
                case "Scenario outline":
                    return GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD;
                case "Примеры":
                case "Examples":
                    return GherkinTokenTypes.EXAMPLES_KEYWORD;
                case "Допустим":
                case "Пусть":
                case "Дано":
                case "Given":
                case "Когда":
                case "Тогда":
                case "И":
                case "And":
                case "Также":
                    return GherkinTokenTypes.STEP_KEYWORD;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyword), keyword);
            }
        }
    }
}