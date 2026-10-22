using UnityEngine;

public static class CompanyRules
{
    public static int CorrectPoints(Company c)
    {
        return c == Company.UPS ? 500 : 100;
    }

    public static int WrongPoints(Company c)
    {
        return -100;
    }

    public static float SignatureTime(Company c)
    {
        return c == Company.UPS ? 3f : 0f;
    }

    public static float DropoffRadiusMultiplier(Company c)
    {
        return c == Company.USPS ? 4f : 1f;
    }

    public static float OrderTimeLimit(Company c)
    {
        return c == Company.FedEx ? 20f : 0f;
    }

    public static bool AutoDeliverOnEnter(Company c)
    {
        return c == Company.USPS;
    }

    public static int FedExOnTimePoints = 200;
}
