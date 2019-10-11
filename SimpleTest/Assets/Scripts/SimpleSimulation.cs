using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Character
{
    public int patience;
    public int power;
    public int waitingInTheQueue;
    public int id;
    static int maxid;

    public Character()
    {
        id = maxid;
        maxid++;
        waitingInTheQueue = 0;

        patience    = Random.Range(5, 95);
        power       = Random.Range(5, 95);
    }

    public Character(int _id, int _patience, int _power)
    {
        id = _id;
        waitingInTheQueue = 0;

        patience = _patience;
        power = _power;
    }
}

public class MM_Queue
{
    public List<Character> characters;

    public MM_Queue(bool refill = false)
    {
        characters = new List<Character>(10);
        if(refill) Refill();
    }

    public void Refill()
    {
        while (characters.Count < 10) characters.Add(new Character());
    }
}

/*
public class Adventure
{
    public int start;
    public int finish;
}
*/

public class Squad
{
    public List<Character> characters = new List<Character>();
    public float funFactor;
    //public Adventure adventure;
    public int ticksSinceActivated;
    public int maxTicks;

    public Squad()
    {
        maxTicks = Random.Range(100, 500);
    }

    public float Reward()
    {
        return 4.0f / (286.0f - (characters[0].power + characters[1].power + characters[2].power) + 60000/(50000 + characters[0].patience* characters[0].waitingInTheQueue) + 60000 / (50000 + characters[1].patience * characters[1].waitingInTheQueue) + 60000 / (50000 + characters[2].patience * characters[2].waitingInTheQueue));
    }
}

public class SimpleSimulation : MonoBehaviour
{
    public MM_Queue queue;

    public List<Squad> squads = new List<Squad>(30);

    public List<MM_SimpleAgent> MM_agents = new List<MM_SimpleAgent>(30);

    public SimpleAcademy academy;
    public MM_SimpleAgent agentPrefab;
    private int step_cnt=0;

    // Start is called before the first frame update
    void Start()
    {
        queue = new MM_Queue(true);
        Debug.LogFormat("INFO: is inference: {0}", academy.GetIsInference());
    }

    // Update is called once per frame
    public void Tick()
    {

        // If any of the MM agents is waiting for a Matchmaker algorithm, 
        // end the simulation to allow Python part to respond.
        foreach (MM_SimpleAgent agent in MM_agents)
        {
            if (agent.isWaitingFromMMmodel)
            {
                Debug.LogFormat("INFO: {0} is waiting for the decision, stopping simulation", agent.name);
                return;
            }
        }

        // Checking for matchmaker results, assigning adventure to matched squads.
        foreach (MM_SimpleAgent agent in MM_agents)
        {
            if (!agent.hasAdventureStarted)
            {
                Squad squad = new Squad();
                squad.characters.Add(agent.queueSnapshot.characters[agent.matchmakedPlayers[0]]);
                squad.characters.Add(agent.queueSnapshot.characters[agent.matchmakedPlayers[1]]);
                squad.characters.Add(agent.queueSnapshot.characters[agent.matchmakedPlayers[2]]);
                squads.Add(squad);

                queue.characters.RemoveAt(agent.matchmakedPlayers[2]);
                queue.characters.RemoveAt(agent.matchmakedPlayers[1]);
                queue.characters.RemoveAt(agent.matchmakedPlayers[0]);

                queue.Refill();

                // Do director's job, assign adventure

                // Mark this squad for adventure simulation
                agent.hasAdventureStarted = true;
            }
        }

        // Warming up till we have 30 squads running on the map.
        if (MM_agents.Count < 3)
        {
            Debug.LogFormat("INFO: MM_agents.Count={0}, Adding squad to the queue", MM_agents.Count);
            AddSquadToMMQueue();
            return;
        }

        // Main adventure simulation logic
        // For now one of the squads can randomly finish their adventure based on ticks

        while (true)
        {
            // Simulation tick for every squad checking whether they finished
            for (var i = 0; i < squads.Count; i++)
            {
                Squad squad = squads[i];

                squad.ticksSinceActivated++;

                if (squad.ticksSinceActivated >= squad.maxTicks)
                {
                    //Squad has finished their adventure
                    squad.funFactor = Random.value;
                    MM_agents[i].SetReward(squad.funFactor);
                    MM_agents[i].CustomAgentReset();
                    MM_agents[i].AssignMMQueue(ref queue);
                    squad.maxTicks = Random.Range(100, 1000);
                    squad.ticksSinceActivated = 0;

                    MM_agents[i].isWaitingFromMMmodel = true;
                    return;
                }
            }

            foreach (Character _char in queue.characters)
                _char.waitingInTheQueue++;
        }
    }

    void AddSquadToMMQueue()
    {
        MM_SimpleAgent agent = Instantiate<MM_SimpleAgent>(agentPrefab);
        agent.name = string.Format("MM Agent {0}", MM_agents.Count);
        Debug.LogFormat("INFO: init new agent: {0}", agent.name);

        agent.CustomAgentReset();
        agent.AssignMMQueue(ref queue);
        agent.isWaitingFromMMmodel = true;

        MM_agents.Add(agent);
        Debug.Log("INFO: New agent is added");
    }
}