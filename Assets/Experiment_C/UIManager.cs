using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI spawnTotalTextMesh;
    [SerializeField] private TextMeshProUGUI modeTextMesh;
    [SerializeField] private TextMeshProUGUI fpsCounter;

    private float deltaTime = 0.0f;

    void Update()
    {
        DisplayCurrentFPS();
    }

    public void DisplayInstanceMode(InstanciationMode mode)
    {
        modeTextMesh.text = mode.ToString();
    }

    public void DisplayTotalInstanced(int totalNumber)
    {
        spawnTotalTextMesh.text = "Instanciated: " + totalNumber;
    }

    private void DisplayCurrentFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        string fpsText = Mathf.Ceil(fps).ToString();

        fpsCounter.text = "FPS: " + fpsText;
    }

}
