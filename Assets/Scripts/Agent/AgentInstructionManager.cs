using Newtonsoft.Json;
using UnityEngine;

public class AgentInstructionManager : MonoBehaviour
{
    //For test
    [SerializeField]
    Grid _grid;

    AgentActionController _actionController;

    private void Start()
    {
        _actionController = GetComponent<AgentActionController>();
    }

    //private void Update()
    //{
    //    Vector3Int pos = _grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    //    //pos.x += 7;
    //    //pos.y -= 3;
    //    //Debug.Log((pos.x + 7) + "  " + (3 - pos.y));
    //    Debug.Log(pos);
    //    if (Input.GetMouseButtonDown(0) && !_actionController.IsBusy())
    //    {
    //        List<AgentCommand> _instructions = new List<AgentCommand> { new AgentCommand(ACTION_TYPE.E_MOVETO, pos) };
    //        _actionController.ReceiveCommands(_instructions);
    //    }
    //}

    public string ParseLLMResponse(string jsonString)
    {
        jsonString = jsonString.Replace("```json", "").Replace("```", "").Trim();

        AgentResponse response = JsonConvert.DeserializeObject<AgentResponse>(jsonString);
        if (response != null)
        {
            Debug.Log($"[LLM 답변]: {response.answer}");

            if (response.commands != null && response.commands.Count > 0)
            {
                _actionController.ReceiveCommands(response.commands);
            }

            return response.answer;
        }
        else
        {
            return "JSON 파싱 실패";
        }
    }
}
