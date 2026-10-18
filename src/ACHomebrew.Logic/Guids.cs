// DO NOT change any value once committed — save compatibility depends on stable GUIDs.
namespace ACHomebrew.Feats
{
    public static class Guids
    {
        public const string ACHomebrewSelection = "849a4e94-bfaa-4c16-9d49-d5fd2096ac05";

        public static class ManeuverTactics
        {
            public const string SeizeOpening = "f35f427c-c793-4f79-9aa0-61b1c088a2df";
            public const string LingeringDisruption = "5a566030-8086-4ce4-9ce4-5d8ce7443967";
            public const string Selection = "974ae1af-6659-4c5c-ad57-7e15ed6165bd";
        }

        public static class FeatSelections
        {
            public const string MainAttribute = "0fca3f71-f928-4a16-bc6d-a7670b7eb497";
            public const string Defensive = "fa0ebb46-169d-4651-8727-d68171da976a";
            public const string Maneuver = "84af69fb-32fe-4bfd-931b-c98349e59157";
            public const string Skilled = "8488006d-b005-4f49-b8be-0c228b5ced5f";
            public const string Arcane = "6c3a2022-f19c-45a2-8cf2-08c47508aab2";
            public const string Stance = "4710986d-ee1f-4eb1-b40a-de2e1ac956db";
            public const string Conditional = "0f1e99f0-b6b1-4172-b8b6-7a57aa0ea227";
            public const string WeaponInsight = "da2b6738-7737-4435-b25f-defbafb26c6b";
            public const string ExtendedReplacement = "3f2263db-3a85-4559-b558-59902482a88d";
            public const string GreaterSummoning = "22872933-4618-433a-a24d-bf1effb041d6";
            public const string SummonerSacrifice = "0cc8f077-1c30-4062-8e39-bb55bba7f2ca";
            public const string ReactiveArmor = "4b5c1fc8-e3d7-4bce-8032-ee70ad9682b1";
            public const string DerivedStat = "cce28abb-3eff-4ecc-83c7-d23d89be7434";
            public const string SpellSchool = "2da24552-6b0c-43c7-8a75-1fc80051ba2c";
            public const string SpellDescriptor = "90b325a4-0c45-4546-a562-52c4503a212b";
            public const string DistanceDamage = "64c8eb89-87bf-4d13-8e05-ffc603c76c64";
            public const string WeaponDamage = "2cdbd2a6-eef6-4d82-a91b-675cbe28e723";
            public const string CastingStat = "c218dfc4-aacf-4f2b-ba98-d358adf682ef";
            public const string ResourceStat = "938135b5-eaaa-42f0-a560-65fef7d1391c";
            public const string Survival = "62c0a731-dba1-47e0-98a4-1b142ea2755e";
            public const string Meme = "8eee7d40-39bf-4db2-bb10-c2cc5ec3e1ec";
            public const string Penetration = "fea01b71-96e7-4f29-bd44-60301b7c4845";
            public const string Retaliation = "35f8879b-c4a0-469d-a797-bbad0f107916";
            public const string Momentum = "50d43f6b-d0b6-4717-9366-d3b01c4f60b3";
            public const string Stealth = "19b5a1e9-9ba6-4d6b-915d-3049b891f5da";
            public const string Solo = "3349bd79-1b70-4c17-8f18-38f687cf97c0";
            public const string Growth = "3c013c4c-3f87-4c0f-bed0-db13a292399e";
            public const string Execution = "35a9dff3-4ebd-4110-a455-f4ef722bef1c";
            public const string Arcana = "5725406d-bf42-4b07-b2cd-008d40883fce";
            public const string Summoner = "393d54be-a075-41d1-9de5-34fb1242a75d";
        }

        public static class WeaponDamage
        {
            public const string Str = "ccfd2d35-5d5f-4e16-ac94-c09408f34ec9";
            public const string Dex = "0b0ad630-9c43-42f3-bc5b-b60aa532681b";
            public const string Con = "16a7b7c2-9149-4472-94c4-6b4837b63089";
            public const string Int = "c7190587-853a-4f83-b2f8-f35a07058a47";
            public const string Wis = "fe9d5c37-632e-41a8-801f-e03fd90e1642";
            public const string Cha = "62b7b1f8-a093-401e-8c84-bb42028cbb59";
        }

        // EXISTING Main feats — DO NOT CHANGE (save compatibility)
        public const string str_main_to_everything = "a4c66462-a423-4a2f-8b26-770ea03d2ce0";
        public const string dex_main_to_everything = "0963babc-0579-4bb3-a33a-23949b47e68b";
        public const string con_main_to_everything = "52506f39-5c40-4780-b677-68336c44dcaa";
        public const string int_main_to_everything = "df0cb753-5704-42a0-bad0-627757af281f";
        public const string wis_main_to_everything = "41584589-3703-41c8-9a51-809805c921ad";
        public const string cha_main_to_everything = "e16525c7-ec29-4904-a69c-8b35f0c60a95";

        // NEW — do not change values once committed
        // Main Attribute Mastery 0.2: source menus and source -> target feats.
        public static class MainAttribute
        {
            public static class Source
            {
                public const string Str = "83e9ef2e-30f7-49fb-a72f-3a7d3eeabc97";
                public const string Dex = "96d93447-bf98-4450-b31d-56b75d7ae437";
                public const string Con = "77ad460f-7188-4dcd-8c2d-e828eb21c963";
                public const string Int = "841a0980-3e52-4f78-9b76-c742f5f9a67a";
                public const string Wis = "dbd431a3-4108-45f9-8561-1c7f883866de";
                public const string Cha = "b7082ff3-927c-4933-ba50-535914d667eb";
            }

            public static class Str
            {
                public const string Dex = "8cea3f12-e942-41ba-9669-9fc7fb48d9d0";
                public const string Con = "52ded285-fd1e-4b63-b4ec-df73fbc96465";
                public const string Int = "d6cb6a76-31f7-4947-89fd-6a4f97b43849";
                public const string Wis = "bc536c29-7689-4497-b7ee-1ea4d0196671";
                public const string Cha = "c7d10485-5317-4719-b48e-d8d0fd1b76b7";
            }

            public static class Dex
            {
                public const string Str = "24615e0e-06f0-4a5d-b076-1426eb4f46a9";
                public const string Con = "3d6bb468-8801-4f63-8247-a6795cbd541f";
                public const string Int = "48e51cea-c48c-4d6f-9c07-58627e664ec7";
                public const string Wis = "cb8bb2e6-35e3-4322-8f1b-50ef90a88ff0";
                public const string Cha = "efe933c2-5a23-46b3-a113-e443ea042b1b";
            }

            public static class Con
            {
                public const string Str = "5af7f34d-2468-4564-9931-568f4dca4fe9";
                public const string Dex = "3731b263-0b92-45d1-8a57-4791d6fdd9ce";
                public const string Int = "edb2326f-80f5-4e95-8175-7346828a1ac4";
                public const string Wis = "d672e017-d41b-4c38-a8bc-d58b451dfbd3";
                public const string Cha = "6efb96f8-286d-4adf-ae9a-1a96e8ef6f70";
            }

            public static class Int
            {
                public const string Str = "dc77ae4b-049e-4927-8399-e3559ac7b377";
                public const string Dex = "51295131-f556-43ff-bca8-235c64302919";
                public const string Con = "e8ce6dd8-06b4-4c35-a90d-5b01e12793eb";
                public const string Wis = "d9188e46-0d40-42bf-a8be-5a472a721e46";
                public const string Cha = "c7b34d2c-af87-4d56-8be0-7ce5fb5583aa";
            }

            public static class Wis
            {
                public const string Str = "c6ae2247-6bef-4199-a39b-328dd01830ec";
                public const string Dex = "ff0a9dfe-53df-4feb-9cc9-73898328c920";
                public const string Con = "3bcaf1ce-939f-4020-aead-efe7b9eb47d1";
                public const string Int = "45056296-ce51-4d3f-8802-586c9e61a6a4";
                public const string Cha = "4caae8d7-84dc-47d8-9495-c3fb6649ba84";
            }

            public static class Cha
            {
                public const string Str = "499aac94-4fa7-449f-bc58-87a911f4257b";
                public const string Dex = "e865b174-1adc-4b83-b97f-49df48a3f990";
                public const string Con = "9a9792bf-3bed-4ed8-91ed-10ec39955886";
                public const string Int = "db05f3d2-5945-48a5-b6b2-ef82fc8325b9";
                public const string Wis = "0a04c1c2-933c-4fa0-9e46-f108993bdfd8";
            }
        }

        public static class CastingStat
        {
            public const string Str = "e07385cb-1884-48b1-ab15-1a8ea02a56d2";
            public const string Dex = "5241ea5e-c0ef-4cb2-bead-9b772715c806";
            public const string Con = "30d7fa02-c172-498c-a221-7be86d26c6f3";
        }

        public static class ResourceStat
        {
            public const string Str = "8517a08e-8754-49ee-aa9f-b750b49fa398";
            public const string Dex = "574d2c8f-dfcf-4645-9788-1ed96b86a301";
            public const string Con = "033bf16d-3d8f-4e72-add7-c5488ad7e3c9";
            public const string Int = "6dfc88b0-43d1-4547-878c-14524d20ff1e";
            public const string Wis = "07b1d140-d77a-40b8-9ed2-e13349b32f28";
            public const string Cha = "89ef1c7a-8625-4a97-8f4b-6ba517c95386";
        }

        // OSS-318 batch 1 playstyle feats, their buffs and kill counters.
        public static class Playstyle
        {
            public const string CounterAttack = "9db23461-1ff8-4404-a3b9-0b3d51e4df63";
            public const string MissCounter = "4422f4ca-05b6-401c-9f4a-93c8f7ea22bc";
            public const string Punisher = "5cac9e2c-aa07-44c6-9c00-53bfe5f0fda2";
            public const string PunisherMarkBuff = "cdcef3f1-13f8-444b-b83b-62c3ca2e7a27";
            public const string ParryCrit = "30dac777-16c8-46be-81fd-7fc2336b9fc4";
            public const string ParryCritBuff = "c86be6f3-0c52-4dc2-a73d-e9f7988d013b";
            public const string Shove = "7a2d7383-28a3-49c7-91ce-e006f8fc8c15";
            public const string KillingSpree = "e4ff3f39-aa86-4d98-ad2f-418ca0a18ea4";
            public const string KillingSpreeBuff = "8af9e8e6-5531-4c03-95d6-70c18602ba9e";
            public const string BattleRhythm = "c43c0c31-1e7a-483f-b571-4fe0e80c0351";
            public const string BattleRhythmBuff = "34ccf177-4751-43fc-abcb-0d0f4895b0a9";
            public const string ReStealth = "4010d909-3561-4155-b328-330c3cc8dd96";
            public const string ReStealthCooldownBuff = "28babf50-421f-4072-92ea-46256366d68b";
            public const string LoneWolf = "54299099-a45f-425e-8526-f150a90aac0a";
            public const string LoneWolfBuff = "57fe88d5-35ad-4b98-b69a-f2739d2285ae";
            public const string EssenceShift = "6685fc4a-0d41-4328-9e76-2765bf48a73a";
            public const string EssenceShiftCounter = "7d585ebf-e403-40ce-9b5a-479fdd89422e";
            public const string FleshHeap = "be2de308-7475-4756-a3b4-fbc53c7cf078";
            public const string FleshHeapCounter = "e808025d-6095-4c7b-bf9d-33455a4242ff";
            public const string ArcaneSiphon = "7438f49b-8f36-4c70-a17b-12b3f16ef165";
            public const string ArcaneSiphonCounter = "c3da67e7-c0da-4d19-8262-1bd0df8aa2a1";
            public const string DevouredVigor = "fcfb6011-188d-43a1-bde8-811cea9afdc0";
            public const string DevouredVigorCounter = "9cebae44-2653-4a84-b1f0-6d107a0d2b43";
        }

        // OSS-318 batch 2: Dota-inspired mechanics.
        public static class Dota
        {
            public const string CorrosiveFinish = "a703f9e3-f64e-4730-92cb-d3ccb7cb1cba";
            public const string PhoenixFury = "753714da-ac49-4947-94f6-9002775add3b";
            public const string FeastSpell = "079b0b01-4ebf-4949-b2d1-f8537bd936ac";
            public const string FeastNatural = "d3521f81-b654-4071-933f-4dcb31af5819";
            public const string FeastCurrent = "b7537968-94c3-4eca-b601-1263a7ff2056";
            public const string Lifesteal = "288d0ff8-3ff5-4b15-8d32-0c021cbf6b5c";
            public const string ManaBreakSpell = "ed7faa60-1346-4166-8fe4-f4949e2d4e99";
            public const string Feast = "700a33eb-883f-47af-a329-8606fcfb30b4";
            public const string SlotHarvest = "a4137eb7-3191-4f1a-91e4-1f15c52b75ba";
            public const string ArcaneOrb = "2be18508-1085-4452-bf28-596f907a1db2";
            public const string EssenceFlux = "5f826c8b-426c-41ec-8586-56446379370e";
            public const string ManaBreak = "35d954ee-b092-440f-91e2-c62aae2ad213";
            public const string Fervor = "1febdb17-692e-4840-85c6-041034e85dac";
            public const string FervorBuff = "49cc7004-4d5a-4d1f-b537-fac25a83b413";
            public const string FervorAttackBuff = "def75a34-a507-45d3-b2b4-989d62cc4812";
            public const string CrushingRhythm = "61e6be19-a6db-4932-a920-991ee8589c25";
            public const string ReturnBlow = "dab031fc-8dfc-4561-9bf2-ca60af7e38cb";
            public const string KrakenShell = "059e96fe-0e52-4b81-ae50-076a5782bb07";
            public const string Necromastery = "d0761e2c-4751-4ba4-b862-1a77dcb5c3d9";
            public const string NecromasteryBuff = "be4e00ab-3216-4471-992a-d390a784b342";
            public const string SpiritLink = "524319d6-0ec5-4392-8c59-56759b1d0e91";
        }

        // Summoning feats added after batch 2.
        public static class Survival
        {
            public const string Undying = "6e0f52b6-c1c0-45ce-a97f-ba98c0044a8c";
            public const string UndyingBuff = "c91d8a11-d93f-48ec-ac04-eebb4f27f4cf";
        }

        public static class Solo
        {
            public const string TrulySolo = "ff26d1a3-6e61-452a-a064-f1525cecf275";
            public const string TrulySoloBuff = "5a8ebd70-f018-487c-8014-46bee02c6b30";
        }

        public static class Meme
        {
            public const string WhatCanISay = "d940f8a8-8289-4896-9ad9-b5f70cb53bec";
            public const string NobodyKnowsBetter = "1ef860f5-784a-4f40-bc9a-90aacfceaa1d";
        }

        public static class Penetration
        {
            public const string MindBreaker = "8c0e895d-a7f2-4861-bb39-d4d85a51a13e";
            public const string Deathbringer = "20b9120d-3c2e-44fa-8c70-887bbc46604e";
            public const string Dread = "8401b032-cee0-4533-aa24-dbb41bfaa279";
            public const string Plaguebearer = "e1d16be0-dbb1-4ca1-b41e-cfbc7a3248ef";
            public const string Paralyzer = "fed1a0fe-5ac5-4ef0-b402-977427ac041d";
            public const string ElementalBreach = "ffb3a410-2098-4eda-941a-8eeb63947a54";
            public const string BoneBreaker = "9eebbe70-02ae-4166-8891-8a539b0a871a";
            public const string HiddenVitals = "62c3ba8b-7f98-46c6-b0d0-5a18ed604709";
        }

        public static class Summoning
        {
            public const string SwarmCaller = "ecf15a97-3285-4d8f-8f5d-7687db5dcf25";
            public const string LingeringBond = "e7106057-57e4-459b-9d3d-69997b2c20e1";
        }

        public static class Specialized
        {
            public static class Defensive
            {
                public const string Str = "2d9033de-faf8-4088-96b0-e8e28df79233";
                public const string Dex = "9333d8d4-497a-4ed1-bfb2-321efaae9741";
                public const string Con = "2b4b7a71-e694-4708-997d-a96bc390802b";
                public const string Int = "d80323de-9919-4d5f-8996-33bec924a61a";
                public const string Wis = "0cfb8868-2f5b-4e84-8113-dca1b7dc7e1f";
                public const string Cha = "0546a1ca-936b-466c-9f75-04f799a35c2e";
            }

            public static class Maneuver
            {
                public const string Str = "8c13097b-b22b-4a99-82b0-83b1b06eeba2";
                public const string Dex = "8b6c4410-00a4-4926-8703-22b664868db4";
                public const string Con = "cc0f6445-9bb9-4c52-9fce-2b73b9bd098c";
                public const string Int = "9796b63c-be1b-47dd-89ab-14dc63ec4b99";
                public const string Wis = "612b166a-5099-4477-b74c-5a07530947df";
                public const string Cha = "68534256-92db-41f1-b391-03c45e1bdc1a";
            }

            public static class Skilled
            {
                public const string Str = "bae75019-4b81-41f6-a5ef-7d948e1994ca";
                public const string Dex = "249b46c1-5afe-436e-9418-afcbabc7f229";
                public const string Con = "856026e2-5bea-4322-974b-9d64f1ad3aa2";
                public const string Int = "95405c22-71a8-40dd-b4a0-92d64b981836";
                public const string Wis = "512e3a70-697d-4a7b-9148-58ba079abc30";
                public const string Cha = "f0918636-a1f3-44fa-b95f-829737730ef5";
            }

            public static class Arcane
            {
                public const string Str = "4e8223b9-3f80-4868-965f-763341d64e13";
                public const string Dex = "abcf29a9-500d-41d3-bc0b-ff57f9840fa6";
                public const string Con = "fdc93d56-6202-4d6b-965b-99065b64e694";
                public const string Int = "3d8a271f-9224-4c6f-bb49-85169ee793fe";
                public const string Wis = "4d637462-b043-4548-833e-6366e7fa6b35";
                public const string Cha = "64517abe-bdd4-46d4-851f-fdeb11daa4a3";
            }
        }

        public static class Stance
        {
            public static class Feature
            {
                public const string Str = "f74543d5-6666-469f-a205-c73599454d64";
                public const string Dex = "48b81512-e4d7-4127-a091-1267362b7312";
                public const string Con = "0e8895ff-d33c-40bd-9797-52a4a84193a5";
                public const string Int = "9b44b583-bd8b-48d0-81b5-3df913f98a80";
                public const string Wis = "53ec3d14-00f9-4d6a-af2e-fd8aa6a77ea6";
                public const string Cha = "e1192d59-6597-49c0-8311-d99490107c93";
            }

            public static class Buff
            {
                public const string Str = "6d631775-a6cc-4217-91c6-7e90c7392e58";
                public const string Dex = "330d5c25-e564-48c9-84b5-ad2bbc4cc94d";
                public const string Con = "a5171b12-4346-40f0-b860-e3f7f0306fd3";
                public const string Int = "e84e92be-9f27-40c4-8ec3-91a018d79116";
                public const string Wis = "4fc0139c-e444-4221-9eca-8048202c11bf";
                public const string Cha = "ad9aff77-da9b-4eb6-b094-e0e9ed3a55f5";
            }

            public static class Activatable
            {
                public const string Str = "dd674856-215d-40d2-9cbc-be2fba4d8f13";
                public const string Dex = "d30242ab-cf40-46bb-ac29-456d27d798a6";
                public const string Con = "c638a34c-e9ac-484a-8853-dd85c0a28d18";
                public const string Int = "a88b3133-7f17-4a06-9227-c106cc397ef9";
                public const string Wis = "0386566e-3fff-4010-b8a8-34d55faba381";
                public const string Cha = "da9409aa-e360-4aef-964e-0b700968a6a9";
            }

            public static class AllyBuff
            {
                public const string CommandingPresence = "5e2e946d-7572-40ac-b901-8f347e3fee5a";
            }

            public static class AreaEffect
            {
                public const string CommandingPresence = "ed66e352-8aee-49e6-8ec2-bb7bba386fee";
            }
        }

        public static class Conditional
        {
            public const string FirstBlood = "102a4418-b8ee-46b9-830d-c66de0dc8ec4";
            public const string EndlessResolve = "23d76b96-2748-4e7b-9bb7-ffb48a00aea4";
            public const string Vendetta = "b4d54a88-e514-4291-9a56-01b66d57ab86";
            public const string PatientHunter = "224ec628-3085-4fdc-aa1f-9c26cf03fe41";

            public static class TriggerBuff
            {
                public const string FirstBlood = "30f90f5c-91b5-4794-b47a-031bc647c48b";
                public const string EndlessResolve = "fd56886a-08aa-4088-86cf-75628422f950";
                public const string Vendetta = "77ce625c-6936-4f84-97ef-396b2dc6260a";
                public const string PatientHunter = "c64eb9ab-c904-4252-9c89-4d13ae1327fb";
            }
        }

        public static class Conditional2 // Berserker's Last Stand, Tactical Reading
        {
            public const string BerserkersLastStand = "eeff8a1f-d105-4aca-9901-b8ca5630f38f";
            public const string TacticalReading = "76b84599-7b1d-44f2-a5f3-fff3e7c65999";

            public static class TriggerBuff
            {
                public const string BerserkersLastStand = "054d1cf0-8118-43ef-acd6-e65c57f36006";
                public const string TacticalReading = "f55c6d0e-87e2-4924-9cbd-ab1114829934";
            }
        }

        public static class Replacement
        {
            public static class WeaponInsight
            {
                public const string Str = "791445d4-206b-415b-b19a-4720440a221a";
                public const string Dex = "16886460-a104-4f66-b6f8-d2779b4e21ca";
                public const string Con = "184c9a80-6a65-4bdc-a312-7a52d6736469";
                public const string Int = "34b8fa42-4923-4c30-ae70-1bc0398b6399";
                public const string Wis = "704d20f1-1ff1-4d9a-9ac6-e4a35bc7f8cd";
                public const string Cha = "c8edba26-a2ca-4137-84ec-ed850295ca50";
            }

            public static class Extended
            {
                public const string InnerSentinel = "b2357477-c2ba-4e32-ad5e-ca56c9a6df4d";
                public const string CalculatedGrip = "07ca886f-fe82-4baa-a84e-495f338b78d2";
                public const string UnyieldingWill = "9c1fb2c2-8732-4348-88c1-f6bfe544dff0";
            }
        }

        public static class ExtendedReplacement2 // 3 more attribute-themed
        {
            public const string BrutalDefender = "e8eb1eba-d6e8-4f55-862f-5485457b5699"; // Str→CMD
            public const string LightfootDefense = "6cfe15bc-b05d-40eb-9434-a9b48d69fcb3"; // Dex→AC unarmored
            public const string IronEndurance = "8254961c-b81a-48ac-9202-e470018d112c"; // Con→HP/level
        }

        public static class ReactiveArmor
        {
            public const string SpikedDefense = "3cca56af-65d0-4854-9bef-8a7b8f44ea89";
            public const string BulwarkOfSteel = "8c842031-fca2-49f0-82c0-2120997c9735";
            public const string BulwarkOfSteelBuff = "513e5225-9e57-4411-8209-9ae948c68b7b";
        }

        public static class Derived
        {
            public const string ArcaneAegis = "b5d28e7e-a229-4c56-ba31-f99fc50ddb5b";
            public const string MartialInsight = "b14dc952-67bc-4943-9ee5-e053ed72d769";
            public const string SkilledDefender = "078125c3-80b2-429a-884f-e98443da7bbf";
            public const string MysticVitality = "f0097731-7349-4dab-ba39-30ed71181e2d";
            public const string SoulBulwark = "0a28d92a-2d2a-4a30-a589-fdb6b93a9e4f";
            public const string SoulBulwarkBuff = "a07d2617-5172-4ff0-b4b8-d3a1441eed98";
            public const string SwordSaint = "0c65b291-ce0a-4d99-b776-b9123f0bc8be";
        }

        public static class Summon
        {
            public static class Feature
            {
                public const string BloodlineOfBeasts = "0e20a30b-710f-44f4-8430-bbff56fff0ec";
                public const string QuickenedPact = "d1055cda-6ef3-4e2c-8bca-c388db283132";
                public const string VitalPact = "2a80ce8d-349a-4bfd-acc3-02ec24232121";
                public const string TacticalBinding = "d41c5951-409d-42dd-9c07-20f87ed5fac8";
                public const string InsightfulSummons = "00a4d128-8b58-42da-b958-a5be2648f530";
                public const string MagneticCalling = "b0c36de7-3739-4c0e-93a2-53f8640c0e93";
            }

            public static class OuterBuff
            {
                public const string BloodlineOfBeasts = "c3968996-caff-4558-b916-7f100125804f";
                public const string QuickenedPact = "6a34f5d7-708c-42d4-b3e4-e6f442c5c0bd";
                public const string VitalPact = "e0322c72-e3a8-4a65-a1f7-282fe40a8d44";
                public const string TacticalBinding = "5a62801c-0179-49e8-a24f-9c6144a9dd29";
                public const string InsightfulSummons = "7603587f-7e94-46ab-bc9b-c9bafe8da6bb";
                public const string MagneticCalling = "e84cbc57-dd11-4332-95ee-2821d5cfb902";
            }

            public static class InnerBuff
            {
                public const string BloodlineOfBeasts = "658df3dd-ebb5-4ccc-8be7-b3d9e5ae36ba";
                public const string QuickenedPact = "2cee59a1-c1a8-4155-a3fb-19e636612df3";
                public const string VitalPact = "a3d197d1-4f8a-4e0b-8c98-4b712c6b7f16";
                public const string TacticalBinding = "99b12003-bb78-4e5f-bded-1e0fd3331eb7";
                public const string InsightfulSummons = "8ed68309-be0a-4fa6-af9e-b93cacb51120";
                public const string MagneticCalling = "5b1a3ba4-0090-43d0-b783-7f0cd4f166de";
            }
        }

        public static class SummonerSacrifice // Family 25
        {
            public static class Feature
            {
                public const string BodyOfMyPact = "83c8873a-40ea-43ed-b279-a56a5ebd03a7";
                public const string DoubledBond = "d442fba8-51c8-48dd-a069-1e0a183fb27f";
                public const string EmpoweredSacrifice = "c54e8935-a046-4d9d-bc8b-276db4ce0c74";
            }

            public static class OuterBuff
            {
                public const string BodyOfMyPact = "2e2be264-a177-4fd1-ae09-4b890c4b62e5";
                public const string DoubledBond = "7828a4de-12ff-4b40-9beb-9e3cc1c057ce";
                public const string EmpoweredSacrifice = "01b11574-0bd9-417c-9b06-110b3a15c16d";
            }

            public static class InnerBuff
            {
                public const string BodyOfMyPact = "8c69d379-3be9-4f4f-8b10-5ae940b4d55e";
                public const string DoubledBond = "84e3137e-a1e8-4338-a334-421f58a24a6e";
                public const string EmpoweredSacrifice = "52c404a4-f11a-476b-954f-a7d39d3ed1a4";
            }
        }

        public static class SpellTag
        {
            public static class School
            {
                public const string PureWarder = "195f83c8-087b-4fa8-80a4-29c5690ac063";
                public const string MasterCaller = "8f56cc10-f632-4b33-848e-a86676e74fc9";
                public const string SeersEdge = "a71bdde1-b1b3-462d-a909-83d8e0cbef6f";
                public const string HeartsTyrant = "1e101e48-83b6-49c2-bb96-2a732e567ff5";
                public const string Spellforge = "5ebe6887-36eb-44e4-9306-d6e6e5e28745";
                public const string Veilweaver = "0c87afdb-534a-4e55-a89a-2cd83e46171d";
                public const string DeathSpeaker = "14436457-df09-4f0e-b016-3413d96132f1";
                public const string ShapeShifter = "2e15db39-ff78-432f-bffe-656a6d310ca3";
            }

            public static class Descriptor
            {
                public const string InnerFlame = "f911da43-689d-4f5d-b444-7e33978356a0";
                public const string FrozenHeart = "0456a92c-2e46-490f-8057-e61186426e22";
                public const string StormChannel = "c740ed3e-53b3-4619-b0cd-fde8827af9dc";
                public const string EtchingMind = "8626c648-1c99-4022-ac10-ce3fe3e60341";
                public const string ResonantVoice = "f5949405-4682-4911-96ba-fe7754ba951b";
            }
        }

        public static class SpellTagDescriptor2 // 4 more descriptors
        {
            public const string EthericMind = "fc85d95a-79af-4607-9404-fd0b9f8ff117"; // Force, Int
            public const string RadiantSoul = "4abca6af-b57f-47a5-bf87-d216ad6ccbf6"; // Positive Energy, Cha
            public const string HollowHeart = "4230836e-24d4-4a2b-9b59-59787a4f220b"; // Negative Energy, Wis
            public const string SubtleTyrant = "628e7fd3-75be-4b67-b785-c4e0bc6247ea"; // Mind-Affecting, Cha
        }

        public static class PolearmMaster // Family 12
        {
            public static class Feature
            {
                public const string PolearmMaster = "85389c3d-b584-4cdf-b12a-cac0fd766796";
            }
        }

        public static class DistanceDamage // Family 24
        {
            public const string AggressorsEdge = "2d2024c2-4944-4036-9649-a1f6702ca084";
            public const string MarksmansFocus = "499dd915-5ee9-4d3b-acf8-484e401d9835";
            public const string OptimalRange = "f05016c7-5126-4053-bc6b-0283d9b28eca";
            public const string ShortBlade = "a69e9196-e0dc-469d-942e-4ed94417dc73";
            public const string CloseQuarters = "8da2c389-77a2-498a-aa83-04ac27480bf1";
            public const string LongHaft = "c4f9dc8b-cf65-472f-ac28-91a090f236de";
            public const string ReachControl = "7cf97e84-1ecb-4ef5-ba2f-9b096eb824af";
            public const string StrongBow = "7cb66f0f-d2b6-4dd1-9647-92970f2b99c2";
            public const string SteadySniper = "868619aa-6900-4a27-833c-41ca702680e6";

            public static class Buff
            {
                public const string FlatBonus = "96c9d080-65e6-4339-9019-d0fa3c63ae48";
            }
        }
    }
}
