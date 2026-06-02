using System;
using LoopboundIdle.Kingdom.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LoopboundIdle.Kingdom.Presentation
{
    [DisallowMultipleComponent]
    public sealed class KingdomDebugController : MonoBehaviour
    {
        [Serializable]
        public sealed class KingdomStringEvent : UnityEvent<string>
        {
        }

        [SerializeField]
        private KingdomGameController gameController;

        [SerializeField]
        private double resourceGrantAmount = 1000d;

        [SerializeField]
        private double collapseTestResourceAmount = 10000d;

        public KingdomStringEvent balanceReportGenerated = new KingdomStringEvent();

        private KingdomDebugTools debugTools;

        public string LastBalanceReportMarkdown
        {
            get { return EnsureDebugTools().LastBalanceReportMarkdown; }
        }

        public void GrantResource(int resourceId)
        {
            if (!Enum.IsDefined(typeof(ResourceId), resourceId))
            {
                return;
            }

            EnsureDebugTools().GrantResource((ResourceId)resourceId, resourceGrantAmount);
        }

        public void GrantResourceAmount(int resourceId, double amount)
        {
            if (!Enum.IsDefined(typeof(ResourceId), resourceId))
            {
                return;
            }

            EnsureDebugTools().GrantResource((ResourceId)resourceId, amount);
        }

        public void GrantAllResources()
        {
            EnsureDebugTools().GrantAllResources(resourceGrantAmount);
        }

        public void GrantCollapseTestResources()
        {
            EnsureDebugTools().GrantCollapseTestResources(collapseTestResourceAmount);
        }

        public void GrantActiveChallengeGoals()
        {
            EnsureDebugTools().GrantActiveChallengeGoals();
        }

        public void GrantChallengeGoals(int challengeId)
        {
            if (!Enum.IsDefined(typeof(ChallengeId), challengeId))
            {
                return;
            }

            EnsureDebugTools().GrantChallengeGoals((ChallengeId)challengeId);
        }

        public void ForceSave()
        {
            EnsureDebugTools().ForceSave();
        }

        public void ForceLoad()
        {
            EnsureDebugTools().ForceLoad();
        }

        public void DeleteLocalSave()
        {
            EnsureDebugTools().DeleteLocalSave();
        }

        public void ResetState()
        {
            EnsureDebugTools().ResetState();
        }

        public void GenerateBalanceReport()
        {
            var markdown = EnsureDebugTools().GenerateBalanceReportMarkdown();
            balanceReportGenerated.Invoke(markdown);
        }

        private KingdomDebugTools EnsureDebugTools()
        {
            if (debugTools != null)
            {
                return debugTools;
            }

            if (gameController == null)
            {
                gameController = GetComponent<KingdomGameController>();
            }

            if (gameController == null)
            {
                throw new InvalidOperationException("KingdomDebugController requires a KingdomGameController.");
            }

            if (gameController.Game == null)
            {
                gameController.RefreshViewModel();
            }

            debugTools = new KingdomDebugTools(gameController.Game);
            return debugTools;
        }
    }
}
