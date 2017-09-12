using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorWindowTimer {

	public enum TimerStatus
    {
        NotStarted,
        Running,
        Over
    }

    private TimerStatus currentStatus = TimerStatus.NotStarted;
    /// <summary>
    /// Timer duration (in seconds)
    /// </summary>
    private float length = 0;
    private float elapsedTime = 0;
    private float stepPrecision = .01f;

    public TimerStatus CurrentStatus
    {
        get
        {
            return currentStatus;
        }

        private set
        {
            currentStatus = value;
        }
    }

    public float Length
    {
        get
        {
            return length;
        }

        set
        {
            if(CurrentStatus == TimerStatus.Running)
            {
                ElapsedTime = 0;
            }

            length = value;
        }
    }

    public float ElapsedTime
    {
        get
        {
            return elapsedTime;
        }

        private set
        {
            elapsedTime = value;
        }
    }

    public float StepPrecision
    {
        get
        {
            return stepPrecision;
        }

        set
        {
            stepPrecision = value;
        }
    }

    public EditorWindowTimer(float length, float stepPrecision = .01f)
    {
        this.Length = length;
        this.StepPrecision = stepPrecision;
    }

    public void Reset()
    {
        this.CurrentStatus = TimerStatus.NotStarted;
        this.ElapsedTime = 0;
    }

    public void Start()
    {
        this.CurrentStatus = TimerStatus.Running;
    }

    public void Pause()
    {
        this.CurrentStatus = TimerStatus.NotStarted;
    }

    public TimerStatus Update()
    {
        if (this.CurrentStatus == TimerStatus.Running)
        {
            this.ElapsedTime += this.StepPrecision;

            if(this.ElapsedTime >= this.Length)
            {
                this.CurrentStatus = TimerStatus.Over;
            }
        }

        return this.CurrentStatus;

    }
}
