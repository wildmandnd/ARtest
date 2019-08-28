using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SimpleAgent : Agent
{
    float targetValue;

    float[] lastObservations;
    float[] lastRewards;


    public override void AgentReset()
    {
        Debug.Log("Agent reset");
        targetValue = Random.value;
        lastObservations = new float[3];
        lastRewards = new float[3];

        for(int i=0;i<3;i++)
        {
            float val = Random.value;
            lastObservations[i] = val;
            lastRewards[i] = Reward(val);
        }
    }

    public override void CollectObservations()
    {
        AddVectorObs(lastObservations[0]);
        AddVectorObs(lastObservations[1]);
        AddVectorObs(lastObservations[2]);
        AddVectorObs(lastRewards[0]);
        AddVectorObs(lastRewards[1]);
        AddVectorObs(lastRewards[2]);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log(vectorAction[0]);
        // Debug.Log(textAction);

        float reward = Reward(vectorAction[0]);
        SetReward(reward);

        lastObservations[2] = lastObservations[1];
        lastRewards[2] = lastRewards[1];
        lastObservations[1] = lastObservations[0];
        lastRewards[1] = lastRewards[0];
        lastObservations[0] = vectorAction[0];
        lastRewards[0] = reward;

        if (Mathf.Abs(vectorAction[0] - targetValue) < 0.03) Done();
    }

    float Reward(float obs)
    {
        float reward = Mathf.Sqrt(1 - Mathf.Abs(obs - targetValue));
        return (obs < 0 || obs > 1) ? 0 : reward;
    }
}
