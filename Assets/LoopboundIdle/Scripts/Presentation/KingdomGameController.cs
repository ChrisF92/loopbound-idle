using System;
using System.IO;
using LoopboundIdle.Kingdom.Persistence;
using UnityEngine;
using UnityEngine.Events;

namespace LoopboundIdle.Kingdom.Presentation
{
    [DisallowMultipleComponent]
    public sealed class KingdomGameController : MonoBehaviour
    {
        [Serializable]
        public sealed class KingdomViewModelEvent : UnityEvent<KingdomGameViewModel>
        {
        }

        [Serializable]
        public sealed class KingdomStringEvent : UnityEvent<string>
        {
        }

        [SerializeField]
        private string saveFileName = KingdomFileSaveStore.DefaultFileName;

        [SerializeField]
        private bool loadOnStart = true;

        [SerializeField]
        private bool tickAutomatically = true;

        [SerializeField]
        private float autosaveIntervalSeconds = 30f;

        [SerializeField]
        private bool saveOnPause = true;

        [SerializeField]
        private bool saveOnQuit = true;

        public KingdomViewModelEvent viewModelChanged = new KingdomViewModelEvent();
        public KingdomStringEvent exportTextCreated = new KingdomStringEvent();
        public KingdomStringEvent errorChanged = new KingdomStringEvent();

        private KingdomGame game;
        private float autosaveTimer;
        private string lastExportText = "";

        public KingdomGame Game
        {
            get { return game; }
        }

        public KingdomGameViewModel ViewModel
        {
            get { return game != null ? game.ViewModel : null; }
        }

        public string LastExportText
        {
            get { return lastExportText; }
        }

        public string LastError
        {
            get { return game != null ? game.LastError : ""; }
        }

        public string SavePath
        {
            get { return BuildSavePath(); }
        }

        private void Awake()
        {
            EnsureGame();
        }

        private void Start()
        {
            EnsureGame();

            if (loadOnStart && File.Exists(BuildSavePath()) && !game.Load())
            {
                PublishError();
                PublishViewModel();
                return;
            }

            PublishError();
            PublishViewModel();
        }

        private void Update()
        {
            EnsureGame();

            if (tickAutomatically)
            {
                game.Advance(Time.deltaTime);
            }

            TickAutosave();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && saveOnPause)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit)
            {
                Save();
            }
        }

        public void NewGame()
        {
            EnsureGame();
            game.NewGame();
            autosaveTimer = 0f;
            PublishError();
        }

        public bool Save()
        {
            EnsureGame();
            var succeeded = game.Save();
            PublishError();
            return succeeded;
        }

        public bool Load()
        {
            EnsureGame();
            var succeeded = game.Load();
            PublishError();
            if (!succeeded)
            {
                PublishViewModel();
            }

            return succeeded;
        }

        public bool DeleteSave()
        {
            EnsureGame();
            var succeeded = game.DeleteSave();
            PublishError();
            return succeeded;
        }

        public bool BuyBuilding(int buildingId)
        {
            EnsureGame();
            var succeeded = game.BuyBuilding(buildingId);
            PublishError();
            return succeeded;
        }

        public bool BuyUpgrade(int upgradeId)
        {
            EnsureGame();
            var succeeded = game.BuyUpgrade(upgradeId);
            PublishError();
            return succeeded;
        }

        public bool StartChallenge(int challengeId)
        {
            EnsureGame();
            var succeeded = game.StartChallenge(challengeId);
            PublishError();
            return succeeded;
        }

        public bool CompleteActiveChallenge()
        {
            EnsureGame();
            var succeeded = game.CompleteActiveChallenge();
            PublishError();
            return succeeded;
        }

        public double CollapseAge()
        {
            EnsureGame();
            var reward = game.CollapseAge();
            PublishError();
            return reward;
        }

        public string ExportSaveText()
        {
            EnsureGame();
            lastExportText = game.ExportSave();
            if (!string.IsNullOrEmpty(lastExportText))
            {
                exportTextCreated.Invoke(lastExportText);
            }

            PublishError();
            return lastExportText;
        }

        public bool ImportSaveText(string saveText)
        {
            EnsureGame();
            var succeeded = game.ImportSave(saveText);
            PublishError();
            if (!succeeded)
            {
                PublishViewModel();
            }

            return succeeded;
        }

        public void RefreshViewModel()
        {
            EnsureGame();
            game.RefreshViewModel();
            PublishError();
        }

        private void EnsureGame()
        {
            if (game != null)
            {
                return;
            }

            game = KingdomGame.CreateWithFileSave(BuildSavePath());
            game.ViewModelChanged += OnGameViewModelChanged;
            PublishViewModel();
        }

        private void TickAutosave()
        {
            if (autosaveIntervalSeconds <= 0f)
            {
                return;
            }

            autosaveTimer += Time.deltaTime;
            if (autosaveTimer < autosaveIntervalSeconds)
            {
                return;
            }

            autosaveTimer = 0f;
            Save();
        }

        private void OnGameViewModelChanged(KingdomGameViewModel viewModel)
        {
            viewModelChanged.Invoke(viewModel);
        }

        private void PublishViewModel()
        {
            if (game != null)
            {
                viewModelChanged.Invoke(game.ViewModel);
            }
        }

        private void PublishError()
        {
            errorChanged.Invoke(LastError ?? "");
        }

        private string BuildSavePath()
        {
            var fileName = string.IsNullOrEmpty(saveFileName) ? KingdomFileSaveStore.DefaultFileName : saveFileName;
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}
