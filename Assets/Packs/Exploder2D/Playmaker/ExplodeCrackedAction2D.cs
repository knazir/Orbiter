// uncomment next line to work with Playmaker
//#define PLAYMAKER
#if PLAYMAKER

// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;
using Exploder2D;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Effects)]
    [Tooltip("Explode cracked objects using Exploder2D")]
    public class ExplodeCracked : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(Exploder2DObject))]
        [Tooltip("The GameObject with an Exploder2D component.")]
        public FsmOwnerDefault gameObject;

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
                    exploder2D.ExplodeCracked(OnExplosion);
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
