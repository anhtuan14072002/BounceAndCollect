using TMPro;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial class RemainingBallCountDisplaySystem : SystemBase
{
    private TextMeshProUGUI _textMeshProUGUI;
    private RectTransform _rectTransform;

    protected override void OnCreate()
    {
        base.OnCreate();
        var textGameObject = GameObject.Find("CounterText");
        if (textGameObject != null)
        {
            _textMeshProUGUI = textGameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    protected override void OnUpdate()
    {
        if (_textMeshProUGUI == null) return;
        Entities
            .WithAll<Wizard.BallSpawnConfigComponent>()
            .ForEach((in Wizard.BallSpawnConfigComponent amount) =>
            {
                _textMeshProUGUI.text = $"{amount.Amount}";
            }).WithoutBurst().Run();
        Entities
            .WithAll<Wizard.SourceCupComponent>()
            .ForEach((in LocalTransform localTransform) =>
            {
                _textMeshProUGUI.transform.position = localTransform.Position;
            }).WithoutBurst().Run();
        
    }
}
