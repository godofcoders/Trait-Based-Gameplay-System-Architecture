using UnityEngine;
using GameplayInfrastructure.Unity;
using GameplayInfrastructure.Core;

namespace GameplayInfrastructure.Demo
{
    public class IgniteTester : MonoBehaviour
    {
        [Tooltip("The entity we want to interact with")]
        public EntityController TargetEntity;

        private void Update()
        {
            // Press Spacebar to ignite the target
            if (Input.GetKeyDown(KeyCode.Space) && TargetEntity != null)
            {
                Debug.Log("🔥 Spacebar pressed: Igniting the entity!");

                // Fetch the decoupled blackboard context
                IAttributeContext context = TargetEntity.GetContext();

                // Change the data state. The BurnableTrait will automatically react.
                context.SetValue(AttributeHashes.IsBurning, 1f);
            }
        }
    }
}