/// <summary>
/// This is a class used to record the state the program is going through
/// </summary>
public class RayTracingResources
{
    public static RayTracingResources Instance = new RayTracingResources();

    /// <summary>
    /// Whether in the play mode or not.
    /// </summary>
    bool isProgramRunning = false;
    public bool IsProgramRunning 
    {
        get { return isProgramRunning; }
        set { isProgramRunning = value; }
    }

    /// <summary>
    /// Whether the camera moving or not.
    /// </summary>
    bool isCamMoving = false;
    public bool IsCamMoving
    {
        get { return isCamMoving; }
        set { isCamMoving = value; }
    }

    /// <summary>
    /// Whether reset the accumulate result
    /// </summary>
    bool isAccumulateReset = false;
    public bool IsAccumulateReset
    {
        get { return isAccumulateReset; }
        set { isAccumulateReset = value; }
    }
}
