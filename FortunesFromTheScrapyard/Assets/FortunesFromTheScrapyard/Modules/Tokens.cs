namespace FortunesFromTheScrapyard.Modules
{
    internal static class Tokens
    {
        public const string agilePrefix = "<style=cIsUtility>Agile</style>";

        public static string agileKeyword = KeywordText("Agile", "The skill can be used while sprinting.");

        public static string slayerKeyword = KeywordText("Slayer", "The skill deals 2% more damage per 1% of health the target has lost, up to <style=cIsDamage>3x</style> damage.");
        public static string ConvertDecimal(float value)
        {
            return (value * 100).ToString() + "%";
        }
        public static string DamageText(string text)
        {
            return $"<style=cIsDamage>{text}</style>";
        }
        public static string DamageValueText(float value)
        {
            return $"<style=cIsDamage>{value * 100}% damage</style>";
        }
        public static string UtilityText(string text)
        {
            return $"<style=cIsUtility>{text}</style>";
        }
        public static string RedText(string text) => HealthText(text);
        public static string HealthText(string text)
        {
            return $"<style=cIsHealth>{text}</style>";
        }
        public static string HealingText(string text)
        {
            return $"<style=cIsHealing>{text}</style>";
        }
        public static string KeywordText(string keyword, string sub)
        {
            return $"<style=cKeywordName>{keyword}</style><style=cSub>{sub}</style>";
        }
        public static string ScepterDescription(string desc)
        {
            return $"\n<color=#d299ff>SCEPTER: {desc}</color>";
        }
        public static string StackText(string text)
        {
            return StackColor($"({text} per stack)");
        }
        public static string StackColor(string text)
        {
            return $"<style=cStack>{text}</style>";
        }
        public static string GetAchievementNameToken(string identifier)
        {
            return $"ACHIEVEMENT_{identifier.ToUpperInvariant()}_NAME";
        }
        public static string GetAchievementDescriptionToken(string identifier)
        {
            return $"ACHIEVEMENT_{identifier.ToUpperInvariant()}_DESCRIPTION";
        }
    }
}