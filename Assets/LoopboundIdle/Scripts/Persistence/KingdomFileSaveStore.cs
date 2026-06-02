using System;
using System.IO;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Persistence
{
    public sealed class KingdomFileSaveStore
    {
        public const string DefaultFileName = "kingdom-save.json";

        private readonly KingdomSaveCodec codec;

        public KingdomFileSaveStore(string savePath)
            : this(savePath, new KingdomSaveCodec())
        {
        }

        public KingdomFileSaveStore(string savePath, KingdomSaveCodec codec)
        {
            if (string.IsNullOrEmpty(savePath))
            {
                throw new ArgumentException("Save path is required.", "savePath");
            }

            this.SavePath = savePath;
            this.codec = codec ?? new KingdomSaveCodec();
        }

        public string SavePath { get; private set; }

        public void Save(KingdomState state, KingdomCatalog catalog, long savedUnixTimeSeconds)
        {
            var directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(SavePath, codec.Serialize(state, catalog, savedUnixTimeSeconds));
        }

        public KingdomState Load(KingdomCatalog catalog)
        {
            if (!File.Exists(SavePath))
            {
                throw new FileNotFoundException("Kingdom save file does not exist.", SavePath);
            }

            return codec.Deserialize(File.ReadAllText(SavePath), catalog);
        }

        public bool TryLoad(KingdomCatalog catalog, out KingdomState state, out string error)
        {
            state = null;
            error = null;

            try
            {
                if (!File.Exists(SavePath))
                {
                    error = "No local save exists.";
                    return false;
                }

                state = Load(catalog);
                return true;
            }
            catch (Exception exception)
            {
                error = "Save load failed: " + exception.Message;
                return false;
            }
        }

        public void Delete()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
    }
}
