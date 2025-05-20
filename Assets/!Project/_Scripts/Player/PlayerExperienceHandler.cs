using System;
using UnityEngine;

public class PlayerExperienceHandler : MonoSingleton<PlayerExperienceHandler>
{
    public float totalGainedXP;
    public float currXP;
    public float xpBreakpoints;
    public float xpInterest => 1f + (float)(Math.Pow(Math.Truncate(currXP / xpBreakpoints),1.4) * 0.1);

    public int level => (int)Math.Truncate(totalGainedXP / xpBreakpoints);

    

    public void GainXP(float xpAmount)
    {
        totalGainedXP += xpAmount * xpInterest;
        currXP += xpAmount * xpInterest;
        
    }

    /// <summary>
    /// spends all of xp and returns number of levels it spended
    /// </summary>
    /// <returns>number of levels it spended</returns>
    public int SpendXP()
    {
        int returnLevel = (int)Math.Truncate(currXP / xpBreakpoints);
        currXP = currXP % xpBreakpoints;
        return returnLevel;
    }

}
