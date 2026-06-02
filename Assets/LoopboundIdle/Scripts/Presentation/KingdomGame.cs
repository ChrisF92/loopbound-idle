using System;
using LoopboundIdle.Kingdom.Core;
using LoopboundIdle.Kingdom.Persistence;

namespace LoopboundIdle.Kingdom.Presentation
{
    public sealed class KingdomGame
    {
        private readonly KingdomSimulator simulator;
        private readonly KingdomSaveCodec saveCodec;
        private readonly KingdomViewModelBuilder viewModelBuilder;
        private KingdomFileSaveStore saveStore;

        public KingdomGame()
            : this(KingdomCatalog.CreateDefault(), null, null)
        {
        }

        public KingdomGame(KingdomCatalog catalog)
            : this(catalog, null, null)
        {
        }

        public KingdomGame(KingdomCatalog catalog, KingdomState state, KingdomFileSaveStore saveStore)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            this.Catalog = catalog;
            this.State = KingdomSaveMigrator.Migrate(state ?? KingdomState.CreateNew(catalog), catalog);
            this.saveStore = saveStore;
            this.simulator = new KingdomSimulator();
            this.saveCodec = new KingdomSaveCodec();
            this.viewModelBuilder = new KingdomViewModelBuilder();
            this.ViewModel = new KingdomGameViewModel();
            RefreshViewModel();
        }

        public event Action<KingdomGameViewModel> ViewModelChanged;

        public KingdomCatalog Catalog { get; private set; }

        public KingdomState State { get; private set; }

        public KingdomGameViewModel ViewModel { get; private set; }

        public double LastOfflineSeconds { get; private set; }

        public string LastError { get; private set; }

        public static KingdomGame CreateWithFileSave(string savePath)
        {
            return new KingdomGame(KingdomCatalog.CreateDefault(), null, new KingdomFileSaveStore(savePath));
        }

        public void SetSaveStore(KingdomFileSaveStore store)
        {
            saveStore = store;
        }

        public void NewGame()
        {
            State = KingdomState.CreateNew(Catalog);
            LastOfflineSeconds = 0d;
            LastError = null;
            RefreshViewModel();
        }

        public void Advance(double seconds)
        {
            simulator.Advance(State, Catalog, seconds);
            RefreshViewModel();
        }

        public double ApplyOfflineProgress()
        {
            return ApplyOfflineProgress(CurrentUnixTimeSeconds());
        }

        public double ApplyOfflineProgress(long currentUnixTimeSeconds)
        {
            LastOfflineSeconds = simulator.AdvanceOffline(State, Catalog, currentUnixTimeSeconds);
            RefreshViewModel();
            return LastOfflineSeconds;
        }

        public bool BuyBuilding(int buildingId)
        {
            if (!Enum.IsDefined(typeof(BuildingId), buildingId))
            {
                LastError = "Unknown building id.";
                return false;
            }

            return BuyBuilding((BuildingId)buildingId);
        }

        public bool BuyBuilding(BuildingId buildingId)
        {
            return RunCommand(() => simulator.TryBuyBuilding(State, Catalog, buildingId), "Could not buy building.");
        }

        public bool BuyUpgrade(int upgradeId)
        {
            if (!Enum.IsDefined(typeof(UpgradeId), upgradeId))
            {
                LastError = "Unknown upgrade id.";
                return false;
            }

            return BuyUpgrade((UpgradeId)upgradeId);
        }

        public bool BuyUpgrade(UpgradeId upgradeId)
        {
            return RunCommand(() => simulator.TryBuyUpgrade(State, Catalog, upgradeId), "Could not buy upgrade.");
        }

        public bool StartChallenge(int challengeId)
        {
            if (!Enum.IsDefined(typeof(ChallengeId), challengeId))
            {
                LastError = "Unknown challenge id.";
                return false;
            }

            return StartChallenge((ChallengeId)challengeId);
        }

        public bool StartChallenge(ChallengeId challengeId)
        {
            return RunCommand(() => simulator.StartChallenge(State, Catalog, challengeId), "Could not start challenge.");
        }

        public bool CompleteActiveChallenge()
        {
            return RunCommand(() => simulator.TryCompleteActiveChallenge(State, Catalog), "Could not complete active challenge.");
        }

        public double CollapseAge()
        {
            try
            {
                var reward = simulator.CollapseAge(State, Catalog);
                LastError = null;
                RefreshViewModel();
                return reward;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                RefreshViewModel();
                return 0d;
            }
        }

        public bool Save()
        {
            return Save(CurrentUnixTimeSeconds());
        }

        public bool Save(long currentUnixTimeSeconds)
        {
            if (saveStore == null)
            {
                LastError = "No save store is configured.";
                return false;
            }

            try
            {
                saveStore.Save(State, Catalog, currentUnixTimeSeconds);
                LastError = null;
                RefreshViewModel();
                return true;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                return false;
            }
        }

        public bool Load()
        {
            return Load(CurrentUnixTimeSeconds());
        }

        public bool Load(long currentUnixTimeSeconds)
        {
            if (saveStore == null)
            {
                LastError = "No save store is configured.";
                return false;
            }

            string error;
            KingdomState loadedState;
            if (!saveStore.TryLoad(Catalog, out loadedState, out error))
            {
                LastError = error;
                return false;
            }

            State = loadedState;
            LastError = null;
            ApplyOfflineProgress(currentUnixTimeSeconds);
            return true;
        }

        public string ExportSave()
        {
            return ExportSave(CurrentUnixTimeSeconds());
        }

        public string ExportSave(long currentUnixTimeSeconds)
        {
            try
            {
                LastError = null;
                return saveCodec.Export(State, Catalog, currentUnixTimeSeconds);
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                return "";
            }
        }

        public bool ImportSave(string saveText)
        {
            return ImportSave(saveText, CurrentUnixTimeSeconds());
        }

        public bool ImportSave(string saveText, long currentUnixTimeSeconds)
        {
            string error;
            KingdomState importedState;
            if (!saveCodec.TryImport(saveText, Catalog, out importedState, out error))
            {
                LastError = error;
                return false;
            }

            State = importedState;
            LastError = null;
            ApplyOfflineProgress(currentUnixTimeSeconds);
            return true;
        }

        public void RefreshViewModel()
        {
            ViewModel = viewModelBuilder.Build(State, Catalog, simulator, LastOfflineSeconds);
            var handler = ViewModelChanged;
            if (handler != null)
            {
                handler(ViewModel);
            }
        }

        private bool RunCommand(Func<bool> command, string failureMessage)
        {
            try
            {
                var succeeded = command();
                LastError = succeeded ? null : failureMessage;
                RefreshViewModel();
                return succeeded;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                RefreshViewModel();
                return false;
            }
        }

        private static long CurrentUnixTimeSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
