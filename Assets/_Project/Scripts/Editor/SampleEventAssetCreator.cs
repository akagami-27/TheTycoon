using System.IO;
using UnityEditor;
using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features.Events;

namespace TheTycoon.Editor
{
    /// <summary>
    /// Editor utility to author and generate sample business event ScriptableObject assets.
    /// Creates three canonical events: Major Client, Employee Retention, and Market Expansion.
    /// </summary>
    public static class SampleEventAssetCreator
    {
        private const string EventDataPath = "Assets/_Project/Data/Events";
        private const string SubAssetPath = "Assets/_Project/Data/Events/SubAssets";

        [MenuItem("TheTycoon/Create Sample Event Assets")]
        public static void CreateSampleAssets()
        {
            if (!Directory.Exists(EventDataPath))
            {
                Directory.CreateDirectory(EventDataPath);
            }
            if (!Directory.Exists(SubAssetPath))
            {
                Directory.CreateDirectory(SubAssetPath);
            }

            CreateMajorClientEvent();
            CreateEmployeeRetentionEvent();
            CreateMarketExpansionEvent();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SampleEventAssetCreator] Sample event ScriptableObjects created successfully.");
        }

        private static void CreateMajorClientEvent()
        {
            string eventPath = $"{EventDataPath}/Event_01_MajorClient.asset";

            // Effects for Choice A
            var cashEffectA = ScriptableObject.CreateInstance<CashEffectSO>();
            cashEffectA.name = "Effect_MajorClient_Cash";
            cashEffectA.SetDelta(Money.FromDollars(75000m));
            SaveOrUpdateAsset(cashEffectA, $"{SubAssetPath}/Effect_MajorClient_Cash.asset");

            var repEffectA = ScriptableObject.CreateInstance<ReputationEffectSO>();
            repEffectA.name = "Effect_MajorClient_Rep";
            repEffectA.SetDeltaReputation(5);
            SaveOrUpdateAsset(repEffectA, $"{SubAssetPath}/Effect_MajorClient_Rep.asset");

            // Choice A
            var choiceA = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceA.name = "Choice_MajorClient_Accept";
            choiceA.Configure(
                choiceId: "CHOICE_ACCEPT_CONTRACT",
                choiceTitle: "Accept Exclusive Contract",
                choiceDescription: "Deploy senior consulting teams to lead the digital transformation project.",
                immediateEffects: new ConsequenceEffectSO[] { cashEffectA, repEffectA }
            );
            SaveOrUpdateAsset(choiceA, $"{SubAssetPath}/Choice_MajorClient_Accept.asset");

            // Choice B
            var choiceB = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceB.name = "Choice_MajorClient_Decline";
            choiceB.Configure(
                choiceId: "CHOICE_DECLINE_CONTRACT",
                choiceTitle: "Decline Contract",
                choiceDescription: "Politely pass on the RFP to preserve bandwidth for existing clients."
            );
            SaveOrUpdateAsset(choiceB, $"{SubAssetPath}/Choice_MajorClient_Decline.asset");

            // Main Event
            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.name = "Event_01_MajorClient";
            evt.Configure(
                eventId: "EVT_MAJOR_CLIENT",
                title: "Major Client Contract Opportunity",
                description: "A Fortune 500 enterprise has invited Nexus Corporation to lead a strategic modernization initiative. Accepting yields immediate revenue and prestige, while declining preserves resources for other pursuits.",
                category: EventCategory.Financial,
                weight: 120,
                isUnique: false,
                cooldownMonths: 6,
                choices: new[] { choiceA, choiceB }
            );
            SaveOrUpdateAsset(evt, eventPath);
        }

        private static void CreateEmployeeRetentionEvent()
        {
            string eventPath = $"{EventDataPath}/Event_02_EmployeeRetention.asset";

            // Choice A Condition & Effects
            var minCashCond = ScriptableObject.CreateInstance<MinimumCashConditionSO>();
            minCashCond.name = "Cond_Retention_MinCash";
            minCashCond.SetRequiredCash(Money.FromDollars(30000m));
            SaveOrUpdateAsset(minCashCond, $"{SubAssetPath}/Cond_Retention_MinCash.asset");

            var cashEffectA = ScriptableObject.CreateInstance<CashEffectSO>();
            cashEffectA.name = "Effect_Retention_Cash";
            cashEffectA.SetDelta(Money.FromDollars(-25000m));
            SaveOrUpdateAsset(cashEffectA, $"{SubAssetPath}/Effect_Retention_Cash.asset");

            var repEffectA = ScriptableObject.CreateInstance<ReputationEffectSO>();
            repEffectA.name = "Effect_Retention_Rep_Boost";
            repEffectA.SetDeltaReputation(10);
            SaveOrUpdateAsset(repEffectA, $"{SubAssetPath}/Effect_Retention_Rep_Boost.asset");

            var choiceA = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceA.name = "Choice_Retention_Bonus";
            choiceA.Configure(
                choiceId: "CHOICE_RETENTION_BONUS",
                choiceTitle: "Issue Retention Bonuses",
                choiceDescription: "Approve executive bonuses and raise compensation packages to protect key talent.",
                requirements: new PrerequisiteConditionSO[] { minCashCond },
                immediateEffects: new ConsequenceEffectSO[] { cashEffectA, repEffectA }
            );
            SaveOrUpdateAsset(choiceA, $"{SubAssetPath}/Choice_Retention_Bonus.asset");

            // Choice B Effects
            var repEffectB = ScriptableObject.CreateInstance<ReputationEffectSO>();
            repEffectB.name = "Effect_Retention_Rep_Loss";
            repEffectB.SetDeltaReputation(-15);
            SaveOrUpdateAsset(repEffectB, $"{SubAssetPath}/Effect_Retention_Rep_Loss.asset");

            var choiceB = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceB.name = "Choice_Retention_LetLeave";
            choiceB.Configure(
                choiceId: "CHOICE_LET_TALENT_LEAVE",
                choiceTitle: "Hold Firm on Budget",
                choiceDescription: "Refuse salary increases and absorb the impact of engineering turnover.",
                immediateEffects: new ConsequenceEffectSO[] { repEffectB }
            );
            SaveOrUpdateAsset(choiceB, $"{SubAssetPath}/Choice_Retention_LetLeave.asset");

            // Main Event
            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.name = "Event_02_EmployeeRetention";
            evt.Configure(
                eventId: "EVT_EMPLOYEE_RETENTION",
                title: "Senior Engineering Retention Crisis",
                description: "Competitors are aggressively poaching core engineering leads with lucrative offers. Management must decide whether to authorize emergency retention funds or face disruptive staff departures.",
                category: EventCategory.Workforce,
                weight: 100,
                isUnique: false,
                cooldownMonths: 4,
                choices: new[] { choiceA, choiceB }
            );
            SaveOrUpdateAsset(evt, eventPath);
        }

        private static void CreateMarketExpansionEvent()
        {
            string eventPath = $"{EventDataPath}/Event_03_MarketExpansion.asset";

            // Choice A Condition & Effects
            var minCashCond = ScriptableObject.CreateInstance<MinimumCashConditionSO>();
            minCashCond.name = "Cond_Expansion_MinCash";
            minCashCond.SetRequiredCash(Money.FromDollars(60000m));
            SaveOrUpdateAsset(minCashCond, $"{SubAssetPath}/Cond_Expansion_MinCash.asset");

            var upfrontCashEffect = ScriptableObject.CreateInstance<CashEffectSO>();
            upfrontCashEffect.name = "Effect_Expansion_UpfrontCash";
            upfrontCashEffect.SetDelta(Money.FromDollars(-50000m));
            SaveOrUpdateAsset(upfrontCashEffect, $"{SubAssetPath}/Effect_Expansion_UpfrontCash.asset");

            var delayedCashEffect = ScriptableObject.CreateInstance<CashEffectSO>();
            delayedCashEffect.name = "Effect_Expansion_DelayedCash";
            delayedCashEffect.SetDelta(Money.FromDollars(100000m));
            SaveOrUpdateAsset(delayedCashEffect, $"{SubAssetPath}/Effect_Expansion_DelayedCash.asset");

            var delayedRepEffect = ScriptableObject.CreateInstance<ReputationEffectSO>();
            delayedRepEffect.name = "Effect_Expansion_DelayedRep";
            delayedRepEffect.SetDeltaReputation(10);
            SaveOrUpdateAsset(delayedRepEffect, $"{SubAssetPath}/Effect_Expansion_DelayedRep.asset");

            var delayedReturn = new DelayedEffectDefinition(delayedCashEffect, delayMonths: 3);
            var delayedRep = new DelayedEffectDefinition(delayedRepEffect, delayMonths: 3);

            var choiceA = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceA.name = "Choice_Expansion_Authorize";
            choiceA.Configure(
                choiceId: "CHOICE_AUTHORIZE_EXPANSION",
                choiceTitle: "Authorize Regional Expansion",
                choiceDescription: "Lease regional facilities, launch targeted advertising, and expand operations.",
                requirements: new PrerequisiteConditionSO[] { minCashCond },
                immediateEffects: new ConsequenceEffectSO[] { upfrontCashEffect },
                delayedEffects: new[] { delayedReturn, delayedRep }
            );
            SaveOrUpdateAsset(choiceA, $"{SubAssetPath}/Choice_Expansion_Authorize.asset");

            // Choice B
            var choiceB = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceB.name = "Choice_Expansion_Consolidate";
            choiceB.Configure(
                choiceId: "CHOICE_CONSOLIDATE_OPERATIONS",
                choiceTitle: "Consolidate Current Markets",
                choiceDescription: "Defer regional expansion and focus resources on core operations."
            );
            SaveOrUpdateAsset(choiceB, $"{SubAssetPath}/Choice_Expansion_Consolidate.asset");

            // Main Event
            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.name = "Event_03_MarketExpansion";
            evt.Configure(
                eventId: "EVT_MARKET_EXPANSION",
                title: "Regional Market Expansion Initiative",
                description: "Market intelligence reveals high demand in an adjacent regional market. Launching an expansion branch requires substantial upfront capital, but promises significant returns in 3 months.",
                category: EventCategory.Market,
                weight: 90,
                isUnique: false,
                cooldownMonths: 12,
                choices: new[] { choiceA, choiceB }
            );
            SaveOrUpdateAsset(evt, eventPath);
        }

        private static void SaveOrUpdateAsset(UnityEngine.Object asset, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
