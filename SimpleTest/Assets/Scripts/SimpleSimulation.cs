using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Character
{
    public int stat1;
    public int stat2;
    public int stat3;
    public int id;
    static int maxid;

    public Character()
    {
        id = maxid;
        maxid++;

        stat1 = Random.Range(1, 100);
        stat2 = Random.Range(1, 100);
        stat3 = Random.Range(1, 100);
    }
}

public class MM_Queue
{
    public List<Character> characters;

    public MM_Queue()
    {
        characters = new List<Character>(10);
        Refill();
    }

    public void RefillAfterMM(MM_SimpleAgent agent)
    {
        //foreach(int _char in agent.)
    }

    public void Refill()
    {
        while (characters.Count < 10) characters.Add(new Character());
        //Debug.Log(characters.Count);
    }
}

public class Adventure
{
    public int start;
    public int finish;
}

public class Squad
{
    public List<Character> characters = new List<Character>();
    public float funFactor;
    public Adventure adventure;
    public int ticksSinceActivated;
    public int maxTicks;

    public Squad()
    {
        maxTicks = Random.Range(100, 1000);
    }

    public float Reward()
    {
        return 1.0f / Mathf.Min(1.0f, Mathf.Abs(adventure.start + adventure.finish - characters[0].stat1 - characters[1].stat2 - characters[2].stat3));
    }
}

public class SimpleSimulation : MonoBehaviour
{
    public MM_Queue queue;

    public List<Squad> squads = new List<Squad>(30);

    public List<MM_SimpleAgent> MM_agents = new List<MM_SimpleAgent>(30);

    public SimpleAcademy academy;
    public MM_SimpleAgent agentPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        queue = new MM_Queue();
        Debug.Log(academy.GetIsInference());
    }

    // Update is called once per frame
    public void Tick()
    {
        // If any of the MM agents is waiting for a Matchmaker algorithm, 
        // end the simulation to allow Python part to respond.
        foreach(MM_SimpleAgent agent in MM_agents)
        {
            if (agent.isWaitingFromMMmodel)
            {
                Debug.LogFormat("{0} is waiting for the decision, stopping simulation", agent.name);
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

                // Do director's job, assign adventure

                // Mark this squad for adventure simulation
                agent.hasAdventureStarted = true;
            }
        }

        // Warming up till we have 30 squads running on the map.
        if (MM_agents.Count < 2)
        {
            Debug.Log("Adding squad to the queue");
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
                    MM_agents[i].isWaitingFromMMmodel = true;
                    return;
                }
            }
        }
    }

    void AddSquadToMMQueue()
    {
        MM_SimpleAgent agent = Instantiate<MM_SimpleAgent>(agentPrefab);
        agent.name = string.Format("MM Agent {0}", MM_agents.Count);
        Debug.LogFormat("init {0}", agent.name);

        agent.CustomAgentReset();
        agent.AssignMMQueue(queue);
        agent.isWaitingFromMMmodel = true;

        MM_agents.Add(agent);
    }
}