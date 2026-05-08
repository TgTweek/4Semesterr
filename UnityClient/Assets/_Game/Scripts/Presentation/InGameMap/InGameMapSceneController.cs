using System;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Run;
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
        [SerializeField] private GameObject defeatPanel = null!;

        [Header("Victory Map Buttons")]
        [SerializeField] private Button combat1Button = null!;
        [SerializeField] private Button combat2Button = null!;
        [SerializeField] private Button combat3Button = null!;
        [SerializeField] private Button returnHomeButton = null!;
        [SerializeField] private Button continueRouteButton = null!;

        [Header("Victory UI")]
        [SerializeField] private TMP_Text victorySummaryText = null!;

        [Header("Defeat UI")]
        [SerializeField] private TMP_Text defeatSummaryText = null!;
        [SerializeField] private Button defeatReturnHomeButton = null!;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText = null!;

        private AuthTokenStore _authTokenStore = null!;
        private RunSessionStore _runSessionStore = null!;
        private RunApiClient _runApiClient = null!;

        private bool _isBusy;

        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();
            _runSessionStore = new RunSessionStore();
            _runApiClient = new RunApiClient(apiBaseUrl, _authTokenStore);

            RegisterButton(combat1Button, () => OnCombatNodeClicked(1));
            RegisterButton(combat2Button, () => OnCombatNodeClicked(2));
            RegisterButton(combat3Button, () => OnCombatNodeClicked(3));

            RegisterButton(returnHomeButton, OnReturnHomeClicked);
            RegisterButton(continueRouteButton, OnContinueRouteClicked);
            RegisterButton(defeatReturnHomeButton, OnDefeatReturnHomeClicked);
        }

        private void Start()
        {
            if (!_authTokenStore.HasAccessToken())
            {
                SceneManager.LoadScene(loginSceneName);
                return;
            }

            Render();
        }

        private void Render()
        {
            SetStatus(string.Empty);

            var lastOutcome = _runSessionStore.GetLastOutcome();

            var isDefeat = string.Equals(
                lastOutcome,
                RunSessionStore.OutcomeDefeat,
                StringComparison.OrdinalIgnoreCase);

            SetActive(victoryPanel, !isDefeat);
            SetActive(defeatPanel, isDefeat);

            if (isDefeat)
            {
                RenderDefeat();
                return;
            }

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
                    $"Battles won this run: {battlesWon}\n" +
                    $"Route progress: {completedInCurrentRoute}/3\n" +
                    $"Gold collected: {totalGold}\n" +
                    $"XP collected: {totalExperience}\n\n" +
                    BuildVictoryInstructionText(isFreshRun, completedInCurrentRoute, canChooseHomeOrContinue);
            }
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

            SceneManager.LoadScene(boardSceneName);
        }

        private void OnContinueRouteClicked()
        {
            if (_isBusy)
            {
                return;
            }

            var battlesWon = _runSessionStore.GetBattlesWon();
            var completedInCurrentRoute = battlesWon % 3;

            if (battlesWon <= 0 || completedInCurrentRoute != 0)
            {
                SetStatus("You can only continue the route after 3 battles.");
                return;
            }

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
            SetButtonInteractable(defeatReturnHomeButton, !isBusy);
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