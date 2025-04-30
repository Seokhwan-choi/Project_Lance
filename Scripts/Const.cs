using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    static class Const
    {
        public static string DefaultActiveButtonFrame = "Button_Yellow";
        public static string DefaultInactiveButtonFrame = "Button_GrayBrown";
        public static string PassAllReceiveActiveButtonFrame = "Button_Deco_Pixel_Yellow_1";
        public static string PassAllReceiveInactiveButtonFrame = "Button_Deco_Pixel_GrayBrown_1";
        public static string PassPurchaseActiveButtonFrame = "Button_Deco_Pixel_Green_1";
        public static string PassPurchaseInactiveButtonFrame = "Button_Deco_Pixel_GrayBrown_1";

        public static string CostumeEquipButtonFrame = "Button_Deco_Pixel_Yellow_1";
        public static string CostumeEquippedButtonFrame = "Button_Deco_Pixel_GrayBrown_1";
        public static string CostumePurchaseActiveButtonFrame = "Button_Deco_Pixel_Blue_1";
        public static string CostumePurchaseInactiveButtonFrame = "Button_Deco_Pixel_GrayBrown_1";

        public static string DefaultActiveTextColor = "2A2220";
        public static string DefaultInactiveTextColor = "FFFFFF";
        public static string EnoughTextColor = "FFFFFF";
        public static string NotEnoughTextColor = "D0D0D0";
        public static string NotEnoughLottoCount = "8F8B88";
        public static float DefaultKnockbackTime = 0.5f;

        public static float Acceleration = 3f;
        public static int EquipmentTypeCount = 4;

        public static float CameraDefaultMovePower = 2.5f;
        public static float CameraJoustingMovePower = 1f;

        public static string ColorGrade_A = "59B1FB";
        public static string ColorGrade_S = "AA66C5";
        public static string ColorGrade_SS = "FBD636";
        public static string ColorGrade_SSS = "D95763";


        public static int TrainSupportBeginnerStart = 25;
        public static int TrainSupportBeginnerEnd = 400;
        public static int TrainSupportIntermediateStart = 450;
        public static int TrainSupportIntermediateEnd = 950;
        public static int TrainSupportAdvancedStart = 1000;
        public static int TrainSupportAdvancedEnd = 1350;


        public static string[] DoTweenKillIgnores = new string[]
            {
                "SleepModeCharacter", "BGMManager", "SkillAutocast",
                "Guide_Highlight_Box", "Guide_Highlight_Circle", "Guide_Highlight_TouchFinger",
                "WeekendGlow_1", "WeekendGlow_2",
                "Jousting_SelectAttack_Head_1", "Jousting_SelectAttack_Head_2",
                "Jousting_SelectAttack_Body_1", "Jousting_SelectAttack_Body_2",
                "Jousting_SelectAttack_Arm_1", "Jousting_SelectAttack_Arm_2",
                "CharacterMessage_SkipText","Highlight_SkipText",
            };
    }
}