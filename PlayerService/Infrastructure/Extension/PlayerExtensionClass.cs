namespace Infrastructure.Extension;

public static class PlayerExtensionClass
{
    public static Dictionary<int,int> levelsystem = SetLevelSystem();
    public static (int, int) CalculateLevelandExperiance(this int currentExperiance, int additionalExperiance, int currentlvl)
    {

        int levelExperiance = levelsystem[currentlvl];

        if (currentExperiance + additionalExperiance > levelExperiance)
        {

            currentlvl++;
            if (currentlvl == 30)
            {
                return (30, currentExperiance + additionalExperiance);
            }
            else
            {
                return (currentlvl, (currentExperiance + additionalExperiance) - levelExperiance);
            }
        }
        else
        {
            return (currentlvl, currentExperiance + additionalExperiance);
        }


    }

    public static Dictionary<int, int> SetLevelSystem()
    {
        var system = new Dictionary<int, int>();

        for(int i = 1; i <= 30;i++)
        {
            system.Add(i, 500+(50*i));
        }



        return system;
    }

}
