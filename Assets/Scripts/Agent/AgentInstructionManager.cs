using System.Collections.Generic;
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

    private void Update()
    {
        Vector3Int pos = _grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        //pos.x += 7;
        //pos.y -= 3;
        if(Input.GetMouseButtonDown(0) && !_actionController.IsBusy())
        {
            List<AgentCommand> _instructions = new List<AgentCommand> { new AgentCommand(ACTION_TYPE.E_MOVETO, pos) };
            _actionController.ReceiveCommands(_instructions);
        }
    }
}
