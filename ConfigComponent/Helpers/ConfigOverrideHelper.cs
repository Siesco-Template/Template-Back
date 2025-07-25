using Newtonsoft.Json.Linq;

namespace ConfigComponent.Helpers
{
    public static class ConfigOverrideHelper
    {
        public static JObject ApplyOverridesToFullConfig(JObject defaultConfig, JArray userOverrides)
        {
            var result = (JObject)defaultConfig.DeepClone();

            foreach (JObject overrideEntry in userOverrides.Cast<JObject>())
            {
                foreach (var prop in overrideEntry.Properties()) // { "theme.mode": "dark" }
                {
                    string path = prop.Name;
                    JToken value = prop.Value; // theme.mode = "dark"

                    ApplyPathOverride(result, path, value);
                }
            }

            return result;
        }

        private static void ApplyPathOverride(JObject root, string path, JToken value)
        {
            var segments = path
                .Replace("]", "")
                .Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries);

            JToken current = root;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];

                if (int.TryParse(segment, out int index) && current is JArray array)
                {
                    while (array.Count <= index)
                        array.Add(JValue.CreateNull());

                    current = array[index];
                }
                else if (current is JObject obj)
                {
                    if (!obj.TryGetValue(segment, out var next) || next.Type == JTokenType.Null)
                    {
                        obj[segment] = new JObject();
                        next = obj[segment];
                    }

                    current = next;
                }
                else
                {
                    return;
                }
            }

            var last = segments.Last();

            if (int.TryParse(last, out int lastIndex) && current is JArray lastArray)
            {
                while (lastArray.Count <= lastIndex)
                    lastArray.Add(JValue.CreateNull());

                lastArray[lastIndex] = value;
            }
            else if (current is JObject lastObj)
            {
                lastObj[last] = value;
            }
        }
    }
}