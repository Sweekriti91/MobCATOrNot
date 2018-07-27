using System;
namespace MobCATOrNot
{
    public static class Constants
    {
        public const string CustomVisionProjectId = "<put your custom vision project id here>";
        public const string CustomVisionTrainingKey = "<put your custom vision training key here>";

        public const string CustomVisionMobCatTag = "mobcat";
        public const string CustomVisionNotMobCatTag = "notmobcat";
        public const double ProbabilityThreshold = 0.3;

        public const string AzureFunctionsUrl = "https://mobcatnotmobcat.azurewebsites.net/api";
        public const string AzureFunctionsCode = "<put your azure functions key here>";

        public const string OptionPickFromLibrary = "Pick from library";
        public const string OptionTakePhoto = "Take a photo";
    }
}
