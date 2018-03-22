// uncomment next line to work with Playmaker
//#define PLAYMAKER
#if PLAYMAKER

// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;
using Exploder2D;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Effects)]
    [Tooltip("Explode objects in the radius using Exploder2D.")]
    public class Explode : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(Exploder2DObject))]
        [Tooltip("The GameObject with an Exploder component.")]
        public FsmOwnerDefault gameObject;

        [Tooltip("Position of the exploder2D")]
        public FsmVector3 Position;

        public override void Reset()
        {
            gameObject = null;
        }

        public override void OnEnter()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go != null)
            {
                var exploder2D = go.GetComponent<Exploder2DObject>();

                if (exploder2D != null)
                {
                    if (!Position.IsNone)
                    {
                        exploder2D.transform.position = Position.Value;
                    }

                    exploder2D.Explode(OnExplosion);
                }
            }
        }

        void OnExplosion(float timeMS, Exploder2DObject.ExplosionState state)
        {
            if (state == Exploder2DObject.ExplosionState.ExplosionFinished)
            {
                Finish();
            }
        }
    }
}

#endif
