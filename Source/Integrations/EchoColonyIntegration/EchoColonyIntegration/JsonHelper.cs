using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Despicable
{
    public static class JsonHelper
    {
        public static JSONNode ExtractSubJsonFromResponse(string fullResponse)
        {
            try
            {
                if (string.IsNullOrEmpty(fullResponse))
                {
                    Log.Warning("[Despicable] - Attempted to parse an empty or null JSON response.");
                    return null;
                }
                JSONNode root = JSON.Parse(fullResponse);
                if (root == null)
                {
                    Log.Warning("[Despicable] - Failed to parse a valid root JSON object from the response.");
                    return null;
                }
                string rawText = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.Value;
                if (rawText == null)
                {
                    Log.Warning("[Despicable] - Could not find the 'text' field in the Gemini API response.");
                    return null;
                }
                int jsonStartIndex = rawText.IndexOf("{");
                if (jsonStartIndex != -1)
                {
                    string jsonString = rawText.Substring(jsonStartIndex);
                    return JSON.Parse(jsonString);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[Despicable] - An unexpected error occurred while extracting JSON: " + ex.Message);
            }
            return null;
        }
    }
}
