using UnityEngine;

public class AnimatorAudioController : MonoBehaviour
{
    bool _isWorking = false;
    public void PlayRobotEffect()
    {
        if(_isWorking) AudioManager.Instance.PlaySFX(SfxType.RobotEffect);
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

    public void SetWorkFlag(bool isWorking)
    {
        _isWorking = isWorking;
    }

    public void PlayWorkEffect() 
    {
        if(_isWorking) AudioManager.Instance.PlaySFX(SfxType.Plant_Harvest);
        else AudioManager.Instance.PlaySFX(SfxType.Bite);
    }
}