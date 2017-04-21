using UnityEngine;

public class TileFadeBehaviour : StateMachineBehaviour
{
    #region OnStateEnter & OnStateUpdate (remove this if it will not become necessary in the near-future)
    /*public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("TileFadeBehaviour.OnStateEnter");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //float elapsedAnimationPlayTime = stateInfo.normalizedTime;

        //if (elapsedAnimationPlayTime >= 1.0f)
        //{
        //Debug.Log("TileFadeBehaviour.OnStateUpdate -> stateInfo.length: " + stateInfo.length);
        //Debug.Log("TileFadeBehaviour.OnStateUpdate -> stateInfo.normalizedTime: " + stateInfo.normalizedTime);
        //EventTrigger.TriggerEvent l = new EventTrigger.TriggerEvent().AddListener(new UnityAction<BaseEventData>());
        //animation on Base Layer is finished do your stuff .(0) is base layer

        //animator.Stop();
        //animator.enabled = false;
        //}
    }*/
    #endregion

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.Stop(); // This will stop it from unnecessarily eating up my memory when the spawning has finished
        animator.enabled = false; // This will stop it from doing anything else (E.g., being active in Animator window)
        Destroy(this); // Just in case this does not get garbage collected automatically
    }
}