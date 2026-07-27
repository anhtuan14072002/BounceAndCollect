using TMPro;
using UnityEngine;

public sealed class SpawnedBallCountView : MonoBehaviour
{
    private TextMeshProUGUI _textMeshProUGUI;
    private Wizard.BallSpawnerAuthoring _stoneGame;

    private void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        _stoneGame = FindObjectOfType<Wizard.BallSpawnerAuthoring>();
    }

    private void Update()
    {
        if (_textMeshProUGUI == null || _stoneGame == null) return;
        _textMeshProUGUI.text = $"{_stoneGame.SpawnedAmount}";
    }
}
