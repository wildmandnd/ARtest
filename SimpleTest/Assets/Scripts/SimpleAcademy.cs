using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SimpleAcademy : Academy
{
    int step;
    public SimpleSimulation ss;

    public override void AcademyReset()
    {
        Debug.Log("INFO: Academy Reset");
    }

    public override void AcademyStep()
    {
        Debug.LogFormat("INFO: Academy Step {0}", step);
        step++;

        ss.Tick();

        base.AcademyStep();
    }
}
