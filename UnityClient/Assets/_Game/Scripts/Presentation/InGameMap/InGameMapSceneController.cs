using System;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Run;
using Game.Infrastructure.Api.Player;
using Game.Infrastructure.Api.Run;
using Game.Infrastructure.Auth;
using Game.Infrastructure.Run;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Presentation.InGameMap.Controllers
{
    public sealed class InGameMapSceneController : MonoBehaviour
    {
        private const string DefaultMerchantId = "22222222-2222-2222-2222-222222222222";

        [Header("Scenes")]
        [SerializeField] private string boardSceneName = "MainGameBoard";
        [SerializeField] private string hubSceneName = "MainHub";
        [SerializeField] private string loginSceneName = "Login";

        [Header("API")]
        [SerializeField] private string apiBaseUrl = "https://localhost:7038";
        [SerializeField] private string merchantId = DefaultMerchantId;

        [Header("Panels")]
        [SerializeField] private GameObject victoryPanel = null!;
        [SerializeField] private GameObject bossMapPanel = null!;
        [SerializeField] private GameObject defeatPanel = null!;

        [Header("Victory Map Buttons")]
        [SerializeField] private Button combat1Button = null!;
        [SerializeField] private Button combat2Button = null!;
        [SerializeField] private Button combat3Button = null!;
        [SerializeField] private Button returnHomeButton = null!;
        [SerializeField] private Button continueRouteButton = null!;

        [Header("Boss Map Buttons")]
        [SerializeField] private Button bossFightButton = null!;
        [SerializeField] private Button bossReturnHomeButton = null!;

        [Header("Victory UI")]
        [SerializeField] private TMP_Text victorySummaryText = null!;

        [Header("Boss UI")]
        [SerializeField] private TMP_Text bossSummaryText = null!;
        [SerializeField] private TMP_Text difficultyText = null!;

        [Header("Defeat UI")]
        [SerializeField] private TMP_Text defeatSummaryText = null!;
        [SerializeField] private Button defeatReturnHomeButton = null!;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText = null!;

        private AuthTokenStore _authTokenStore = null!;
        private RunSessionStore _runSessionStore = null!;
        private RunApiClient _runApiClient = null!;
        private PlayerApiGateway _playerApiGateway = null!;

        private int _difficultyTier;
        private int _highestDifficultyTierReached;
        private int _bossesDefeated;

        private bool _isBusy;

        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();
            _runSessionStore = new RunSessionStore();

            _runApiClient = new RunApiClient(apiBaseUrl, _authTokenStore);
            _playerApiGateway = new PlayerApiGateway(apiBaseUrl, _authTokenStore);

            RegisterButton(combat1Button, () => OnCombatNodeClicked(1));
            RegisterButton(combat2Button, () => OnCombatNodeClicked(2));
            RegisterButton(combat3Button, () => OnCombatNodeClicked(3));

            RegisterButton(returnHomeButton, OnReturnHomeClicked);
            RegisterButton(continueRouteButton, OnContinueRouteClicked);

            RegisterButton(bossFightButton, OnBossFightClicked);
            RegisterButton(bossReturnHomeButton, OnReturnHomeClicked);

            RegisterButton(defeatReturnHomeButton, OnDefeatReturnHomeClicked);
        }

        private async void Start()
        {
            if (!_authTokenStore.HasAccessToken())
            {
                SceneManager.LoadScene(loginSceneName);
                return;
            }

            await LoadPlayerProgressAsync();
            Render();
        }

        private async Task LoadPlayerProgressAsync()
        {
            try
            {
                var player = await _playerApiGateway.GetCurrentPlayerAsync();

                _difficultyTier = Math.Max(0, player.difficultyTier);
                _highestDifficultyTierReached = Math.Max(0, player.highestDifficultyTierReached);
                _bossesDefeated = Math.Max(0, player.bossesDefeated);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not load player difficulty progress: {ex.Message}");

                _difficultyTier = 0;
                _highestDifficultyTierReached = 0;
                _bossesDefeated = 0;
            }
        }

        private void Render()
        {
            SetStatus(string.Empty);

            SetActive(victoryPanel, false);
            SetActive(bossMapPanel, false);
            SetActive(defeatPanel, false);

            RefreshDifficultyText();

            var lastOutcome = _runSessionStore.GetLastOutcome();

            var isDefeat = string.Equals(
                lastOutcome,
                RunSessionStore.OutcomeDefeat,
                StringComparison.OrdinalIgnoreCase);

            if (isDefeat)
            {
                SetActive(defeatPanel, true);
                RenderDefeat();
                return;
            }

            var isBossVictory = string.Equals(
                lastOutcome,
                RunSessionStore.OutcomeBossVictory,
                StringComparison.OrdinalIgnoreCase);

            if (isBossVictory)
            {
                SetActive(bossMapPanel, true);
                SetStatus("Boss defeated. Saving run...");
                _ = CompleteRunAndReturnHomeAsync(RunSessionStore.OutcomeBossVictory);
                return;
            }

            if (_runSessionStore.IsNextFightBoss())
            {
                RenderBossChoice();
                return;
            }

            SetActive(victoryPanel, true);
            RenderVictoryMap();
        }

        private void RenderVictoryMap()
        {
            var battlesWon = _runSessionStore.GetBattlesWon();
            var totalGold = _runSessionStore.GetTotalGold();
            var totalExperience = _runSessionStore.GetTotalExperience();

            var completedInCurrentRoute = battlesWon % 3;
            var isFreshRun = battlesWon == 0;
            var canChooseHomeOrContinue = battlesWon > 0 && completedInCurrentRoute == 0;

            SetButtonVisible(combat1Button, isFreshRun);
            SetButtonVisible(combat2Button, completedInCurrentRoute == 1);
            SetButtonVisible(combat3Button, completedInCurrentRoute == 2);

            SetButtonVisible(returnHomeButton, canChooseHomeOrContinue);
            SetButtonVisible(continueRouteButton, canChooseHomeOrContinue);

            if (victorySummaryText != null)
            {
                victorySummaryText.text =
                    "Battle Map\n\n" +
                    $"World Tier: {_difficultyTier + 1}\n" +
                    $"Highest Tier: {_highestDifficultyTierReached + 1}\n" +
                    $"Bosses defeated: {_bossesDefeated}\n\n" +
                    $"Battles won this run: {battlesWon}\n" +
                    $"Route progress: {completedInCurrentRoute}/3\n" +
                    $"Gold collected: {totalGold}\n" +
                    $"XP collected: {totalExperience}\n\n" +
                    BuildVictoryInstructionText(isFreshRun, completedInCurrentRoute, canChooseHomeOrContinue);
            }
        }

        private void RenderBossChoice()
        {
            SetActive(victoryPanel, false);
            SetActive(bossMapPanel, true);
            SetActive(defeatPanel, false);

            SetButtonVisible(bossFightButton, true);
            SetButtonVisible(bossReturnHomeButton, true);

            var totalGold = _runSessionStore.GetTotalGold();
            var totalExperience = _runSessionStore.GetTotalExperience();
            var battlesWon = _runSessionStore.GetBattlesWon();

            if (bossSummaryText != null)
            {
                bossSummaryText.text =
                    "Boss Battle\n\n" +
                    $"World Tier: {_difficultyTier + 1}\n" +
                    $"Highest Tier: {_highestDifficultyTierReached + 1}\n" +
                    $"Bosses defeated: {_bossesDefeated}\n\n" +
                    $"Battles won this run: {battlesWon}\n" +
                    $"Gold collected: {totalGold}\n" +
                    $"XP collected: {totalExperience}\n\n" +
                    "You have cleared 9 battles.\n" +
                    "Choose Return Home or Fight Boss.";
            }

            RefreshDifficultyText();
        }

        private void RenderDefeat()
        {
            var battlesWon = _runSessionStore.GetBattlesWon();
            var totalGold = _runSessionStore.GetTotalGold();
            var totalExperience = _runSessionStore.GetTotalExperience();

            if (defeatSummaryText != null)
            {
                defeatSummaryText.text =
                    "Defeat\n\n" +
                    $"World Tier: {_difficultyTier + 1}\n" +
                    $"Battles won this run: {battlesWon}\n" +
                    $"Gold to bank: {totalGold}\n" +
                    $"XP to bank: {totalExperience}\n\n" +
                    "You will lose non-starter loadout cards.\n" +
                    "Starter cards, stash, gear, gold, and XP will be kept.";
            }
        }

        private static string BuildVictoryInstructionText(
            bool isFreshRun,
            int completedInCurrentRoute,
            bool canChooseHomeOrContinue)
        {
            if (isFreshRun)
            {
                return "Choose battle 1.";
            }

            if (canChooseHomeOrContinue)
            {
                return "Choose Return Home or Continue Route.";
            }

            if (completedInCurrentRoute == 1)
            {
                return "Choose battle 2.";
            }

            if (completedInCurrentRoute == 2)
            {
                return "Choose battle 3.";
            }

            return "Continue your route.";
        }

        private void OnCombatNodeClicked(int combatNode)
        {
            if (_isBusy)
            {
                return;
            }

            if (_runSessionStore.IsNextFightBoss())
            {
                SetStatus("The boss battle is available. Choose Return Home or Fight Boss.");
                return;
            }

            var battlesWon = _runSessionStore.GetBattlesWon();
            var completedInCurrentRoute = battlesWon % 3;

            var canClickCombat1 = combatNode == 1 && battlesWon == 0;
            var canClickCombat2 = combatNode == 2 && completedInCurrentRoute == 1;
            var canClickCombat3 = combatNode == 3 && completedInCurrentRoute == 2;

            if (!canClickCombat1 && !canClickCombat2 && !canClickCombat3)
            {
                SetStatus("You must complete the route in order.");
                return;
            }

            _runSessionStore.StartNormalFight();
            SceneManager.LoadScene(boardSceneName);
        }

        private void OnContinueRouteClicked()
        {
            if (_isBusy)
            {
                return;
            }

            if (_runSessionStore.IsNextFightBoss())
            {
                SetStatus("The boss battle is available. Choose Return Home or Fight Boss.");
                return;
            }

            var battlesWon = _runSessionStore.GetBattlesWon();
            var completedInCurrentRoute = battlesWon % 3;

            if (battlesWon <= 0 || completedInCurrentRoute != 0)
            {
                SetStatus("You can only continue the route after 3 battles.");
                return;
            }

            _runSessionStore.StartNormalFight();
            SceneManager.LoadScene(boardSceneName);
        }

        private void OnBossFightClicked()
        {
            if (_isBusy)
            {
                return;
            }

            if (!_runSessionStore.IsNextFightBoss())
            {
                SetStatus("Boss battle unlocks after 9 battles.");
                return;
            }

            _runSessionStore.StartBossFight();
            SceneManager.LoadScene(boardSceneName);
        }

        private async void OnReturnHomeClicked()
        {
            await CompleteRunAndReturnHomeAsync(RunSessionStore.OutcomeReturnedHome);
        }

        private async void OnDefeatReturnHomeClicked()
        {
            await CompleteRunAndReturnHomeAsync(RunSessionStore.OutcomeDefeat);
        }

        private async Task CompleteRunAndReturnHomeAsync(string outcome)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Saving run...");

                var request = new CompleteRunRequestDto
                {
                    goldEarned = _runSessionStore.GetTotalGold(),
                    experienceEarned = _runSessionStore.GetTotalExperience(),
                    outcome = outcome,
                    merchantId = merchantId
                };

                var response = await _runApiClient.CompleteRunAsync(request);

                Debug.Log(response.message);

                _runSessionStore.Clear();

                SceneManager.LoadScene(hubSceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                SetStatus(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            SetButtonInteractable(combat1Button, !isBusy);
            SetButtonInteractable(combat2Button, !isBusy);
            SetButtonInteractable(combat3Button, !isBusy);

            SetButtonInteractable(returnHomeButton, !isBusy);
            SetButtonInteractable(continueRouteButton, !isBusy);

            SetButtonInteractable(bossFightButton, !isBusy);
            SetButtonInteractable(bossReturnHomeButton, !isBusy);

            SetButtonInteractable(defeatReturnHomeButton, !isBusy);
        }

        private void RefreshDifficultyText()
        {
            if (difficultyText == null)
            {
                return;
            }

            difficultyText.text =
                $"World Tier {_difficultyTier + 1}\n" +
                $"Highest Tier {_highestDifficultyTierReached + 1}\n" +
                $"Bosses defeated {_bossesDefeated}";
        }

        private static void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void SetButtonVisible(Button button, bool isVisible)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(isVisible);
        }

        private static void SetButtonInteractable(Button button, bool isInteractable)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = isInteractable;
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target == null)
            {
                return;
            }

            target.SetActive(isActive);
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}