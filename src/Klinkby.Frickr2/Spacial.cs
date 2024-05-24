namespace Klinkby.Frickr2
{
    public static class Spacial
    {
        public static (float Degrees, float Minutes, float Seconds) DecimalDegreesToDms(float dd, int numDecimals = 3)
        {
            // https://en.wikipedia.org/wiki/Decimal_degrees
            (float Degrees, float Minutes, float Seconds) dms;
            float t = MathF.Abs(dd);
            dms.Degrees = MathF.Floor(t);
            t = (t - dms.Degrees) * 60f;
            dms.Minutes = MathF.Floor(t);
            t = (t - dms.Minutes) * 60f;
            dms.Seconds = MathF.Round(t, numDecimals);
            dms.Degrees *= MathF.Sign(dd);
            return dms;
        }
    }
}