using UnityEngine;

public class AgentFeedbackManager : MonoBehaviour
{
    [SerializeField]
    ChatFeedback _feedbackUI;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowFeedbackUI(string instruct) { _feedbackUI.ShowFeedbackUI(instruct); }
}
