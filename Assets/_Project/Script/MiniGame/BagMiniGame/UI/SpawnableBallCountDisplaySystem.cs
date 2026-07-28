using TMPro;
using Unity.Entities;
using UnityEngine;

public partial class SpawnableBallCountDisplaySystem : SystemBase
{
    private TextMeshPro _textMeshPro;
    private int _displayedValue = -1;

    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<Wizard.BallSpawnConfigComponent>();
    }

    protected override void OnStartRunning()
    {
        GameObject textGameObject = GameObject.Find("BagText");
        if (textGameObject != null)
            _textMeshPro = textGameObject.GetComponent<TextMeshPro>();
    }

    protected override void OnUpdate()
    {
        if (_textMeshPro == null) return;

        int value = SystemAPI.GetSingleton<Wizard.BallSpawnConfigComponent>().Amount;
        if (value == _displayedValue) return;

        _displayedValue = value;
        _textMeshPro.SetText("{0}", value);
    }
}
