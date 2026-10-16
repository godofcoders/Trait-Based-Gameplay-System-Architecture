using GameplayInfrastructure.Core;
using GameplayInfrastructure.Unity;
using UnityEngine;

namespace GameplayInfrastructure.Demo
{
    public sealed class IgniteTester : MonoBehaviour
    {
        [Tooltip("The entity to ignite. The demo assigns the barrel.")]
        public EntityController TargetEntity;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space) || TargetEntity == null) return;
            var context = TargetEntity.GetContext();
            if (context.HasAttribute(AttributeHashes.IsBurning))
                context.SetValue(AttributeHashes.IsBurning, 1f);
        }

        private void OnGUI()
        {
            // Immediate-mode GUI is intentionally limited to this teaching demo.
            string status = "Barrel destroyed. Stop and restart Play mode to reset.";
            if (TargetEntity != null)
            {
                var context = TargetEntity.GetContext();
                status = $"Health: {context.GetValue(AttributeHashes.Health):0.0} / " +
                    $"{context.GetValue(AttributeHashes.MaxHealth):0.0}    " +
                    $"Burning: {context.GetValue(AttributeHashes.IsBurning) > 0f}";
            }
            GUI.Box(new Rect(12, 12, 510, 58), "Press Space to ignite the barrel.\n" + status);
        }
    }
}
