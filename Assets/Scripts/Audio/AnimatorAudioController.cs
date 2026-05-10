using UnityEngine;

public class AnimatorAudioController : MonoBehaviour
{
    public void PlayRobotEffect()
    {
        AudioManager.Instance.PlaySFX(SfxType.RobotEffect);
    }

    public void PlayWalkEffect()
    {
        int randomWalk = Random.Range(0, 2); // 0 또는 1
        if (randomWalk == 0)
        {
            AudioManager.Instance.PlaySFX(SfxType.Walk0);
        }
        else
        {
            AudioManager.Instance.PlaySFX(SfxType.Walk1);
        }
    }
}