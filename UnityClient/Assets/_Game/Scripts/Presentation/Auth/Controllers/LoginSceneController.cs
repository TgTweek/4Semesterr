using System;
using Game.Infrastructure.Api.Auth;
using Game.Infrastructure.Api.Dtos;
using Game.Infrastructure.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Presentation.Auth.Controllers
{
    public sealed class LoginSceneController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string backendBaseUrl = "https://localhost:7038";
        [SerializeField] private string hubSceneName = "Hub";

        [Header("Panels")]
        [SerializeField] private GameObject loginPanel = null!;
        [SerializeField] private GameObject registerPanel = null!;

        [Header("Tabs")]
        [SerializeField] private Button loginTabButton = null!;
        [SerializeField] private Button registerTabButton = null!;

        [Header("Login Fields")]
        [SerializeField] private TMP_InputField loginEmailInput = null!;
        [SerializeField] private TMP_InputField loginPasswordInput = null!;
        [SerializeField] private Button loginButton = null!;

        [Header("Register Fields")]
        [SerializeField] private TMP_InputField registerEmailInput = null!;
        [SerializeField] private TMP_InputField registerPasswordInput = null!;
        [SerializeField] private TMP_InputField registerPlayerNameInput = null!;
        [SerializeField] private Button registerButton = null!;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText = null!;

        private AuthApiClient _authApiClient = null!;
        private AuthTokenStore _authTokenStore = null!;
        private bool _isBusy;

        private void Awake()
        {
            _authApiClient = new AuthApiClient(backendBaseUrl);
            _authTokenStore = new AuthTokenStore();

            loginTabButton.onClick.AddListener(ShowLoginPanel);
            registerTabButton.onClick.AddListener(ShowRegisterPanel);
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);

            ShowLoginPanel();
            SetStatus(string.Empty);
        }

        private void ShowLoginPanel()
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            SetStatus(string.Empty);
        }

        private void ShowRegisterPanel()
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
            SetStatus(string.Empty);
        }

        private async void OnLoginClicked()
        {
            if (_isBusy)
                return;

            var email = loginEmailInput.text.Trim();
            var password = loginPasswordInput.text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                SetStatus("Email and password are required.");
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Logging in...");

                var request = new LoginRequestDto
                {
                    email = email,
                    password = password
                };

                var response = await _authApiClient.LoginAsync(request);
                _authTokenStore.Save(response);

                SetStatus("Login successful.");
                SceneManager.LoadScene(hubSceneName);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnRegisterClicked()
        {
            if (_isBusy)
                return;

            var email = registerEmailInput.text.Trim();
            var password = registerPasswordInput.text;
            var playerName = registerPlayerNameInput.text.Trim();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(playerName))
            {
                SetStatus("Email, password, and player name are required.");
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Registering...");

                var request = new RegisterRequestDto
                {
                    email = email,
                    password = password,
                    playerName = playerName
                };

                var response = await _authApiClient.RegisterAsync(request);
                _authTokenStore.Save(response);

                SetStatus("Registration successful.");
                SceneManager.LoadScene(hubSceneName);
            }
            catch (Exception ex)
            {
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

            loginTabButton.interactable = !isBusy;
            registerTabButton.interactable = !isBusy;
            loginButton.interactable = !isBusy;
            registerButton.interactable = !isBusy;
        }

        private void SetStatus(string message)
        {
            statusText.text = message;
        }
    }
}