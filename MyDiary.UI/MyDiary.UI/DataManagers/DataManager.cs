using System;

public static class DataManager
{
    public static IDataManager dataManager;
    public static IDataManager Manager
    {
        get
        {
            if (dataManager == null)
            {
                if (OperatingSystem.IsBrowser())
                {
                    dataManager = new WebDataManager();
                }
                else
                {
                    dataManager = new LocalDataManager();
                }
            }
            return dataManager;
        }
    }
}
