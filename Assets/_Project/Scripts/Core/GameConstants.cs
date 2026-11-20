namespace TimeMachine.Core
{
    public enum SkillBranch
    {
        Knowledge = 0,
        Speech = 1,
        City = 2,
        PlaceLore = 3
    }

    public static class QuestIds
    {
        public const string Q01 = "Q-01";
        public const string Q02 = "Q-02";
        public const string Q03 = "Q-03";
        public const string Q04 = "Q-04";
        public const string Q05 = "Q-05";
        public const string Q06 = "Q-06";
        public const string Q07 = "Q-07";
        public const string Q08 = "Q-08";
        public const string Q09 = "Q-09";
        public const string Q10 = "Q-10";
        public const string Q11 = "Q-11";
        public const string Q12 = "Q-12";
        public const string Q13 = "Q-13";
        public const string Q14 = "Q-14";
        public const string Q15 = "Q-15";
        public const string Q16 = "Q-16";
        public const string Q17 = "Q-17";
        public const string Q18 = "Q-18";
        public const string Q19 = "Q-19";
        public const string Q20 = "Q-20";
        public const string Q21 = "Q-21";
        public const string Q22 = "Q-22";
        public const string Q23 = "Q-23";
        public const string Q24 = "Q-24";

        public static readonly string[] OrderedChain =
        {
            Q01, Q02, Q03, Q04, Q05, Q06, Q07, Q08,
            Q09, Q10, Q11, Q12,
            Q13, Q14, Q15, Q16,
            Q17, Q18, Q19, Q20,
            Q21, Q22, Q23, Q24
        };
    }

    public static class LocationIds
    {
        public const string Hill = "L-2027-HILL";
        public const string HillWorkbench = "L-2027-HILL_WORKBENCH";
        public const string Kitchen = "L-2027-KITCHEN";
        public const string Courtyard71 = "L-2027-COURTYARD71";
        public const string House57 = "L-2027-HOUSE57";
        public const string House53 = "L-2027-HOUSE53";
        public const string ZimaLeto = "L-2027-ZIMALETO";
        public const string Bridge = "L-2027-BRIDGE";

        public const string Bridge1950 = "L-1950-BRIDGE";
        public const string Zubov1950 = "L-1950-ZUBOV";
        public const string Hill1950 = "L-1950-HILL";
    }

    public static class GameConstants
    {
        public static class CharacterIds
        {
            public const string Sonya2027 = "sonya2027";
            public const string Sonya2040 = "sonya2040";
            public const string Sasha = "sasha";
            public const string Kristina = "kristina";
            public const string Lyova = "lyova";
            public const string Artur = "artur";
            public const string Babushka = "babushka";
            public const string Barni = "barni";
            public const string Zhulik = "zhulik";
        }

        public static class ItemIds
        {
            public const string Note1950 = "item_note_1950";
            public const string PartQuartzStabilizer = "part_quartz_stabilizer";
            public const string PartMagneticCoil = "part_magnetic_coil";
            public const string PartCapacitorBlock = "part_capacitor_block";
            public const string PartChronoCore = "part_chrono_core";
            public const string PartMicrowaveMotor = "part_microwave_motor";
            public const string GenerationsCompass = "gadget_generations_compass";
            public const string Blueprint1950 = "item_blueprint_1950";
            public const string Zarya7Assembled = "item_zarya7_assembled";
            public const string CopperShield1950 = "item_copper_shield_1950";
            public const string Zarya7DualReady = "item_zarya7_dual_ready";
            public const string JuniorEngineerBadge = "item_junior_engineer_badge";

            public static readonly string[] RequiredZarya7Parts =
            {
                PartChronoCore,
                PartMicrowaveMotor,
                PartMagneticCoil,
                PartCapacitorBlock,
                PartQuartzStabilizer
            };
        }

        public static class GameplayTuning
        {
            public const float WalkSpeed = 2.2f;
            public const float RunSpeed = 3.0f;
            public const float CameraPitch = 15.0f;
            public const float CameraDistance = 4.2f;
            public const float CameraHeight = 1.65f;
            public const float MinTouchSizeDp = 48.0f;
            public const int MaxSkillLevel = 3;
            public const int XpPerSkillLevel = 100;
            public const int TotalZarya7MassGrams = 3000;
            public const int EraShiftYears = 77;
            public const int CurrentSaveSchemaVersion = 1;
        }
    }
}
