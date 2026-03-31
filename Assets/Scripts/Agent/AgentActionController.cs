using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ACTION_TYPE
{
    E_MOVETO,
    E_HARVEST,
}

// JSON을 파싱하여 생성할 데이터 클래스
[System.Serializable]
public class AgentCommand
{
    public ACTION_TYPE Action;
    public Vector3Int TargetGridPos;

    public AgentCommand() { }
    public AgentCommand(ACTION_TYPE act, Vector3Int target)
    {
        Action = act;
        TargetGridPos = target;
    }
}

public class AgentActionController : MonoBehaviour
{
    Transform _agent;

    [SerializeField]
    bool _isBusy = false;
    [SerializeField]
    Grid _grid;
    [SerializeField]
    float _moveSpeed = 2f;

    private Queue<AgentCommand> _commandQueue = new Queue<AgentCommand>();
    private Coroutine _actionCoroutine;

    private void Start()
    {
        _agent = transform.parent;
    }

    public bool IsBusy() { return _isBusy; }

    public void ReceiveCommands(List<AgentCommand> commands)
    {
        foreach (var cmd in commands)
        {
            _commandQueue.Enqueue(cmd);
        }

        if (!_isBusy)
        {
            _actionCoroutine = StartCoroutine(ProcessCommandsCoroutine());
        }
    }

    private IEnumerator ProcessCommandsCoroutine()
    {
        _isBusy = true;

        while (_commandQueue.Count > 0)
        {
            AgentCommand currentCommand = _commandQueue.Dequeue();

            switch (currentCommand.Action)
            {
                case ACTION_TYPE.E_MOVETO:
                    yield return StartCoroutine(MoveToRoutine(currentCommand.TargetGridPos));
                    break;
                case ACTION_TYPE.E_HARVEST:
                    yield return StartCoroutine(HarvestRoutine(currentCommand.TargetGridPos));
                    break;
            }
        }

        _isBusy = false;
        _actionCoroutine = null;
    }

    private IEnumerator MoveToRoutine(Vector3Int targetPos)
    {
        Vector3 targetWorldPos = _grid.GetCellCenterWorld(targetPos);
        targetWorldPos.z = _agent.position.z;

        while (Vector3.Distance(_agent.position, targetWorldPos) > 0.01f)
        {
            _agent.position = Vector3.MoveTowards(_agent.position, targetWorldPos, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        _agent.position = targetWorldPos;
    }

    private IEnumerator HarvestRoutine(Vector3Int targetPos)
    {
        yield return new WaitForSeconds(1f);
    }
}