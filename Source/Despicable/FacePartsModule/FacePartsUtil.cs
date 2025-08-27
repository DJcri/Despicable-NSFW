namespace Despicable
{
    /// <summary>
    /// Internal configuration for face part handling
    /// AND general HELPER FUNCTIONS for FACE PARTS
    /// </summary>
    public static class FacePartsUtil
    {
        public static readonly string TexPathBase = "FaceParts/";
        public static readonly string GenderedTag = "Gendered/";

        public static int expressionUpdateInterval = 60;
        public static int updateTickResetOn = 10000;
        // Blink interval should always be lower than update intervel reset on
        public static int blinkInterval = 1000;
        public static int blinkTickVariance = 240;
    }
}