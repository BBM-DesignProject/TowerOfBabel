using UnityEngine;
    using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli

    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Names")]
        [Tooltip("Name of the main game scene to load.")]
        public string mainGameSceneName = "MainGame"; // Oyun sahnenizin adını buraya yazın

        // Oyuna Başla butonu tıklandığında çağrılacak metod
        public void StartGame()
        {
            if (!string.IsNullOrEmpty(mainGameSceneName))
            {
                Debug.Log($"Starting game, loading scene: {mainGameSceneName}");
                SceneManager.LoadScene(mainGameSceneName);
            }
            else
            {
                Debug.LogError("MainGameSceneName is not set in the MainMenuManager component in the Inspector!");
            }
        }

        // Ayarlar butonu tıklandığında (şimdilik boş, daha sonra doldurulabilir)
        public void OpenOptions()
        {
            Debug.Log("Options button clicked - Not implemented yet.");
            // Burada bir ayarlar paneli açılabilir.
        }

        // Çıkış butonu tıklandığında çağrılacak metod
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();

            // Unity Editor'deyken Application.Quit() hemen çalışmaz.
            // Editörde test etmek için aşağıdaki satırı kullanabilirsiniz:
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }