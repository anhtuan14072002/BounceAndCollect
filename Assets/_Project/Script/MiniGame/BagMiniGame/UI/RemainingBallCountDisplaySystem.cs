using TMPro;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial class RemainingBallCountDisplaySystem : SystemBase
{
    private TextMeshPro _textMeshPro;
    private RectTransform _rectTransform;

    protected override void OnCreate()
    {
        base.OnCreate();
        var textGameObject = GameObject.Find("CounterText");
        if (textGameObject != null)
        {
            _textMeshPro = textGameObject.GetComponent<TextMeshPro>();
        }
    }

    protected override void OnUpdate()
    {
        if (_textMeshPro == null) return;
        Entities
            .WithAll<Wizard.BallSpawnConfigComponent>()
            .ForEach((in Wizard.BallSpawnConfigComponent amount) =>
            {
                _textMeshPro.text = $"{amount.Amount}";
            }).WithoutBurst().Run();
        Entities
            .WithAll<Wizard.SourceCupComponent>()
            .ForEach((in LocalTransform localTransform) =>
            {
                _textMeshPro.transform.position = localTransform.Position;
            }).WithoutBurst().Run();
        
    }
}
