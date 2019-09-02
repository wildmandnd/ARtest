using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class MM_SimpleAgent : Agent
{
    public MM_Queue queueSnapshot;
    public float MM_reward = float.MinValue;
    public int[] matchmakedPlayers;
    public bool isWaitingFromMMmodel = false;
    public bool hasAdventureStarted = false;

    public void CustomAgentReset()
    {
        Debug.Log("Agent reset");
        MM_reward = float.MinValue;
        matchmakedPlayers = new int[3];
        queueSnapshot = new MM_Queue();
    }

    public void AssignMMQueue(MM_Queue _queue)
    {
        queueSnapshot.characters.Clear();
        //Debug.LogFormat("fillin queue from global MM queue of {0}", _queue.characters.Count);
        queueSnapshot.characters.AddRange(_queue.characters);
    }

    public override void CollectObservations()
    {
        foreach(Character _char in queueSnapshot.characters)
        {
            //Debug.LogFormat("Adding observations of {0}", _char.id);
            AddVectorObs(_char.stat1);
            AddVectorObs(_char.stat2);
            AddVectorObs(_char.stat3);
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (!isWaitingFromMMmodel || 0 == vectorAction[2])
        {
            Debug.LogFormat("{0} has received unintended Action callback, ignoring", name);
            return;
        }

        int player1 = Mathf.FloorToInt(vectorAction[0]);
        int player2 = Mathf.FloorToInt(vectorAction[1]);
        int player3 = Mathf.FloorToInt(vectorAction[2]);

        Debug.LogFormat("Matching players {0}, {1} and {2}", player1, player2, player3);

        if (
            player1 < 0 || player1 > 9 ||
            player2 < 0 || player2 > 9 ||
            player3 < 0 || player3 > 9 ||

            player1 == player2 || player2 == player3 || player3 == player1
            )
        {
            SetReward(0.0f);
            Done();
            return;
        }

        matchmakedPlayers[0] = player1;
        matchmakedPlayers[1] = player2;
        matchmakedPlayers[2] = player3;

        isWaitingFromMMmodel = false;

        /*
        if (MM_reward >= 0)
        {
            SetReward(MM_reward);
            Done();
        }
        */
    }
}
