using System;
using System.Text;
using LoopboundIdle.Kingdom.Core;
using UnityEngine;

namespace LoopboundIdle.Kingdom.Persistence
{
    public sealed class KingdomSaveCodec
    {
        public const string ExportPrefix = "LOOPBOUND-KINGDOM-SAVE:1:";

        public string Serialize(KingdomState state, KingdomCatalog catalog, long savedUnixTimeSeconds)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            state.lastSavedUnixTimeSeconds = Math.Max(0L, savedUnixTimeSeconds);
            KingdomSaveMigrator.Migrate(state, catalog);
            return JsonUtility.ToJson(state);
        }

        public KingdomState Deserialize(string json, KingdomCatalog catalog)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("Save JSON is empty.", "json");
            }

            if (!LooksLikeKingdomStateJson(json))
            {
                throw new ArgumentException("Save JSON does not look like Kingdom state data.", "json");
            }

            var state = JsonUtility.FromJson<KingdomState>(json);
            return KingdomSaveMigrator.Migrate(state, catalog);
        }

        public string Export(KingdomState state, KingdomCatalog catalog, long savedUnixTimeSeconds)
        {
            var json = Serialize(state, catalog, savedUnixTimeSeconds);
            return ExportPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public KingdomState Import(string saveText, KingdomCatalog catalog)
        {
            string error;
            KingdomState state;
            if (!TryImport(saveText, catalog, out state, out error))
            {
                throw new ArgumentException(error, "saveText");
            }

            return state;
        }

        public bool TryImport(string saveText, KingdomCatalog catalog, out KingdomState state, out string error)
        {
            state = null;
            error = null;

            try
            {
                string json;
                if (!TryExtractJson(saveText, out json, out error))
                {
                    return false;
                }

                state = Deserialize(json, catalog);
                return true;
            }
            catch (Exception exception)
            {
                error = "Save import failed: " + exception.Message;
                state = null;
                return false;
            }
        }

        public bool TryExtractJson(string saveText, out string json, out string error)
        {
            json = null;
            error = null;

            if (string.IsNullOrEmpty(saveText))
            {
                error = "Save text is empty.";
                return false;
            }

            var trimmed = saveText.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                if (!LooksLikeKingdomStateJson(trimmed))
                {
                    error = "Save JSON does not look like Kingdom state data.";
                    return false;
                }

                json = trimmed;
                return true;
            }

            if (!trimmed.StartsWith(ExportPrefix, StringComparison.Ordinal))
            {
                error = "Save text is not a recognized Loopbound Kingdom save.";
                return false;
            }

            var encoded = trimmed.Substring(ExportPrefix.Length);
            try
            {
                json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                if (!LooksLikeKingdomStateJson(json))
                {
                    error = "Save payload did not decode to Kingdom state JSON.";
                    json = null;
                    return false;
                }

                return true;
            }
            catch (FormatException)
            {
                error = "Save export text is not valid base64.";
                return false;
            }
        }

        private static bool LooksLikeKingdomStateJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var trimmed = json.Trim();
            return trimmed.StartsWith("{", StringComparison.Ordinal) &&
                trimmed.EndsWith("}", StringComparison.Ordinal) &&
                trimmed.IndexOf("\"saveVersion\"", StringComparison.Ordinal) >= 0 &&
                trimmed.IndexOf("\"wallet\"", StringComparison.Ordinal) >= 0 &&
                trimmed.IndexOf("\"buildings\"", StringComparison.Ordinal) >= 0;
        }
    }
}
