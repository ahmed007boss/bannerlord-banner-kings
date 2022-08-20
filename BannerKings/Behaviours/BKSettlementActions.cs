﻿using System;
using System.Collections.Generic;
using BannerKings.Managers.Skills;
using BannerKings.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKSettlementActions : CampaignBehaviorBase
    {
        private static float actionGold;
        private static int actionHuntGame;
        private static CampaignTime actionStart = CampaignTime.Now;
        private float totalHours;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu("bannerkings", "Banner Kings", MenuBannerKingsInit);
            campaignGameStarter.AddGameMenu("bannerkings_actions", "Banner Kings", MenuBannerKingsInit);

            // ------- WAIT MENUS --------

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_guard",
                "{=!}You are serving as a guard in {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuGuardActionPeasantCondition,
                MenuActionConsequenceWithGold,
                TickWaitGuard, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_guard", "wait_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_train_guards",
                "{=!}You are training the guards in {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuTrainGuardActionPeasantCondition,
                MenuActionConsequenceWithGold,
                TickWaitTrainGuard, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_train_guards", "wait_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);


            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_hunt",
                "{=!}You are hunting in the region of {CURRENT_SETTLEMENT}. Game quantity in this region is {HUNTING_GAME}.",
                MenuWaitInit,
                MenuHuntingActionCondition,
                MenuActionHuntingConsequence,
                TickWaitHunt, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_hunt", "wait_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_meet_nobility",
                "{=!}You are meeting with the high society of {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuMeetNobilityActionCondition,
                MenuActionMeetNobilityConsequence,
                TickWaitMeetNobility, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 4f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_meet_nobility", "wait_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_study",
                "{=!}You are studying scholarship with {SCHOLARSHIP_TUTOR}. The instruction costs {SCHOLARSHIP_GOLD} per hour.",
                MenuWaitInit,
                MenuActionStudyCondition,
                MenuActionConsequenceNeutral,
                TickWaitStudy, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 4f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_study", "wait_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);


            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_crafting",
                "{=!}You are working on the smith for {CRAFTING_HOURS} hours. The current hourly rate of this smith is: {CRAFTING_RATE} {GOLD_ICON}.{CRAFTING_EXPLANATION}",
                MenuWaitInit,
                c => true,
                MenuActionConsequenceNeutral,
                TickWaitCrafting, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth);


            // ------- ACTIONS --------

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_study", "{=!}Study scholarship",
                MenuActionStudyCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_study"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_slave_transfer", "{=!}Transfer slaves",
                MenuSlavesActionCondition, delegate { UIHelper.ShowSlaveTransferScreen(); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_meet_nobility", "{=!}Meet nobility",
                MenuMeetNobilityActionCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_meet_nobility"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_guard", "{=!}Serve as guard",
                MenuGuardActionPeasantCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_guard"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_train_guards", "{=!}Train guards",
                MenuTrainGuardActionPeasantCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_train_guards"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_hunt", "{=!}Go hunting",
                MenuHuntingActionCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_hunt"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "bannerkings_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate { GameMenu.SwitchToMenu("bannerkings"); }, true);


            // ------- TOWN --------

            campaignGameStarter.AddGameMenuOption("town", "bannerkings_submenu", "{=!}Banner Kings",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null &&
                           BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate { GameMenu.SwitchToMenu("bannerkings"); }, false, 4);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_demesne", "{=!}Demesne management",
                MenuSettlementManageCondition,
                MenuSettlementManageConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_titles", "{=!}Demesne hierarchy",
                MenuTitlesCondition,
                MenuTitlesConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_faith", "{=!}{RELIGION_NAME}",
                MenuFaithCondition,
                MenuFaithConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_guild", "{=!}{GUILD_NAME}",
                MenuGuildCondition,
                MenuGuildManageConsequence);


            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_action", "{=!}Take an action",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                },
                delegate { GameMenu.SwitchToMenu("bannerkings_actions"); });

            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_leave", "{=3sRdGQou}Leave",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate
                {
                    var menu = Settlement.CurrentSettlement.IsVillage ? "village" :
                        Settlement.CurrentSettlement.IsCastle ? "castle" : "town";
                    GameMenu.SwitchToMenu(menu);
                }, true);


            // ------- CASTLE --------


            campaignGameStarter.AddGameMenuOption("castle", "bannerkings_castle_submenu", "{=!}Banner Kings",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null &&
                           BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate { GameMenu.SwitchToMenu("bannerkings"); }, false, 4);

            campaignGameStarter.AddGameMenuOption("bannerkings", "castle_recruit_volunteers", "{=E31IJyqs}Recruit troops",
                MenuCastleRecruitsCondition,
                delegate(MenuCallbackArgs args) { args.MenuContext.OpenRecruitVolunteers(); },
                false, 3);


            // ------- VILLAGE --------


            campaignGameStarter.AddGameMenuOption("village", "bannerkings_village_submenu", "{=!}Banner Kings",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null &&
                           BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate { GameMenu.SwitchToMenu("bannerkings"); }, false, 2);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_projects", "{=!}Village Projects",
                MenuVillageBuildingCondition,
                MenuVillageProjectsConsequence, false, 2);
        }


        // -------- TICKS ----------

        private static void TickWaitGuard(MenuCallbackArgs args, CampaignTime dt)
        {
            TickCheckHealth();
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);

                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var settlement = Settlement.CurrentSettlement;
                    float wage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(3);
                    wage *= settlement.Prosperity / 8000f;
                    actionGold += wage;

                    var random = MBRandom.RandomFloat;
                    var injury = 0.1f;

                    if (settlement.Town == null)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}Not a town!").ToString()));
                        Hero.MainHero.HitPoints -= MBRandom.RandomInt(3, 10);
                        return;
                    }

                    injury -= settlement.Town.Security * 0.001f;
                    if (random <= injury)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have been hurt in your current action.").ToString()));
                        Hero.MainHero.HitPoints -= MBRandom.RandomInt(3, 10);
                    }

                    var skill = 0.1f;
                    skill += settlement.Town.Security * 0.001f;
                    if (random <= skill)
                    {
                        var skills = new List<(SkillObject, float)>();
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.OneHanded, 10f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.TwoHanded, 2f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Polearm, 8f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Bow, 4f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Crossbow, 4f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Athletics, 2f));

                        var target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have improved your {SKILL} skill during your current action.")
                                    .ToString()));
                    }
                }
            }
        }

        public void StartCraftingMenu(float totalHours)
        {
            this.totalHours = totalHours;
            MBTextManager.SetTextVariable("CRAFTING_HOURS", totalHours.ToString("0.0"));
            var cost = BannerKingsConfig.Instance.SmithingModel.GetSmithingHourlyPrice(Settlement.CurrentSettlement,
                Hero.MainHero);
            var costInt = (int) cost.ResultNumber;
            GameTexts.SetVariable("CRAFTING_RATE", costInt);
            GameTexts.SetVariable("CRAFTING_EXPLANATION", cost.GetExplanations());
            GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            GameMenu.SwitchToMenu("bannerkings_wait_crafting");
        }

        private static void TickWaitStudy(MenuCallbackArgs args, CampaignTime dt)
        {
            TickCheckHealth();
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.25f);

                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var main = Hero.MainHero;
                    var seller = Campaign.Current.GetCampaignBehavior<BKEducationBehavior>()
                        .GetBookSeller(Settlement.CurrentSettlement);
                    if (seller != null)
                    {
                        main.AddSkillXp(BKSkills.Instance.Scholarship, 5f);
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, seller,
                            (int) BannerKingsConfig.Instance.EducationModel.CalculateLessionCost(Hero.MainHero, seller)
                                .ResultNumber);
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=!}You have improved your {SKILL} skill during your current action.")
                                .SetTextVariable("SKILL", BKSkills.Instance.Scholarship.Name)
                                .ToString()));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject(
                                    "{=!}You have stopped your current action because the instructor has left the settlement.")
                                .ToString()));
                        GameMenu.SwitchToMenu("bannerkings_actions");
                    }
                }
            }
        }

        private static void TickWaitTrainGuard(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var settlement = Settlement.CurrentSettlement;
                    float wage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(5);
                    wage *= settlement.Prosperity / 8000f;
                    actionGold += wage;

                    var random = MBRandom.RandomFloat;
                    var skill = 0.15f;
                    if (random <= skill)
                    {
                        var skills = new List<(SkillObject, float)>();
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Leadership, 10f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.OneHanded, 2f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Polearm, 2f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Bow, 2f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Crossbow, 2f));

                        var target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have improved your {SKILL} skill during your current action.")
                                    .ToString()));
                    }
                }
            }
        }

        private static void TickWaitHunt(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);

                var settlement = Settlement.CurrentSettlement;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).LandData;
                var woodland = data.Woodland;
                var game = "";
                if (woodland >= 50000)
                {
                    game = new TextObject("{=!}Bountiful").ToString();
                }
                else if (woodland >= 25000)
                {
                    game = new TextObject("{=!}Mediocre").ToString();
                }
                else
                {
                    game = new TextObject("{=!}Poor").ToString();
                }

                GameTexts.SetVariable("HUNTING_GAME", game);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var chance = woodland * 0.001f;
                    var random = MBRandom.RandomFloatRanged(1f, 100f);
                    if (random <= chance)
                    {
                        actionHuntGame += 1;
                        var skills = new List<(SkillObject, float)>();
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Bow, 10f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Crossbow, 5f));
                        skills.Add(new ValueTuple<SkillObject, float>(DefaultSkills.Athletics, 8f));

                        var target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject(
                                        "{=!}You have have caught an animal and improved your {SKILL} skill while hunting.")
                                    .ToString()));
                    }
                }
            }
        }

        private void TickWaitCrafting(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;


            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff / totalHours);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var cost = BannerKingsConfig.Instance.SmithingModel.GetSmithingHourlyPrice(Settlement.CurrentSettlement,
                        Hero.MainHero);
                    var costInt = (int) cost.ResultNumber;
                    GameTexts.SetVariable("CRAFTING_RATE", costInt);
                    GameTexts.SetVariable("CRAFTING_EXPLANATION", cost.GetExplanations());
                    GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
                    GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, Settlement.CurrentSettlement, costInt);
                }
            }
        }

        private static void TickWaitMeetNobility(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.250f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var chance = Hero.MainHero.GetSkillValue(DefaultSkills.Charm) * 0.05f + 15f;
                    var random = MBRandom.RandomFloatRanged(1f, 100f);
                    if (random <= chance)
                    {
                        var influence = MBRandom.RandomFloatRanged(0.1f, 0.5f);
                        GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, influence);
                        GameTexts.SetVariable("INFLUENCE", influence);
                        GameTexts.SetVariable("SKILL", DefaultSkills.Charm.Name);
                        Hero.MainHero.AddSkillXp(DefaultSkills.Charm, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject(
                                    "{=!}You have improved your {SKILL} skill and gained {INFLUENCE} influence while meeting with nobles.")
                                .ToString()));
                    }
                }
            }
        }

        private static void TickCheckHealth()
        {
            if (IsWounded())
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        new TextObject("{=!}You have stopped your current action due to health conditions.").ToString()));
                GameMenu.SwitchToMenu("bannerkings_actions");
            }
        }

        // -------- CONDITIONS ----------

        private static bool IsPeasant()
        {
            return Clan.PlayerClan.Kingdom == null &&
                   (Clan.PlayerClan.Fiefs == null || Clan.PlayerClan.Fiefs.Count == 0);
        }

        private static bool IsWounded()
        {
            return Hero.MainHero.HitPoints / (float) Hero.MainHero.MaxHitPoints <= 0.4f;
        }

        private static bool IsCriminal(Clan ownerClan)
        {
            var criminal = false;
            if (ownerClan != null)
            {
                var kingdom = ownerClan.Kingdom;
                if (kingdom != null)
                {
                    criminal = kingdom.MainHeroCrimeRating > 0;
                }
            }

            return criminal;
        }

        private static bool MenuSlavesActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
            return Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan;
        }

        private static bool MenuWaitActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            return IsPeasant();
        }

        private static bool MenuGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);

            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) &&
                   !Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuActionStudyCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            var seller = Campaign.Current.GetCampaignBehavior<BKEducationBehavior>()
                .GetBookSeller(Settlement.CurrentSettlement);
            var hasSeller = seller != null;
            if (hasSeller)
            {
                MBTextManager.SetTextVariable("SCHOLARSHIP_TUTOR", seller.Name);
                MBTextManager.SetTextVariable("SCHOLARSHIP_GOLD", BannerKingsConfig.Instance.EducationModel
                    .CalculateLessionCost(Hero.MainHero,
                        seller).ResultNumber.ToString());
            }

            return !IsWounded() && hasSeller && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) &&
                   !Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuTrainGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            var leadership = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);
            var combat = Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) +
                         Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) +
                         Hero.MainHero.GetSkillValue(DefaultSkills.Bow) +
                         Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow);
            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && leadership >= 50 &&
                   combat >= 160;
        }

        private static bool MenuMeetNobilityActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            var inFaction = Clan.PlayerClan.Kingdom != null;
            return !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && inFaction;
        }

        private static bool MenuHuntingActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            var criminal = false;
            var huntingRight = false;
            var clan = Settlement.CurrentSettlement.OwnerClan;
            if (clan != null)
            {
                var kingdom = clan.Kingdom;
                if (kingdom != null)
                {
                    criminal = kingdom.MainHeroCrimeRating > 0;
                    huntingRight = kingdom.HasPolicy(DefaultPolicies.HuntingRights);
                }
            }

            return !IsWounded() && !criminal && (huntingRight || Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan);
        }

        private static bool MenuCastleRecruitsCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            var kingdom = Clan.PlayerClan.Kingdom;
            return Settlement.CurrentSettlement.IsCastle && Settlement.CurrentSettlement.Notables.Count > 0 &&
                   (Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan ||
                    kingdom == Settlement.CurrentSettlement.OwnerClan.Kingdom);
        }

        private static bool MenuGuildCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            var settlement = Settlement.CurrentSettlement;
            var hasGuild = false;
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var guild = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).EconomicData.Guild;
                hasGuild = guild != null;
                if (hasGuild)
                {
                    GameTexts.SetVariable("GUILD_NAME", guild.GuildType.Name.ToString());
                }
            }

            return hasGuild;
        }

        private static bool MenuFaithCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            var hasFaith = false;
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement)
                    .ReligionData;
                hasFaith = data != null;
                if (data != null)
                {
                    MBTextManager.SetTextVariable("RELIGION_NAME", data.Religion.Faith.GetFaithName());
                }
            }

            return hasFaith;
        }

        private static bool MenuCourtCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            var currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && !currentSettlement.IsVillage;
        }

        private static bool MenuTitlesCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
            var currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.MapFaction == Hero.MainHero.MapFaction;
        }

        private static bool MenuSettlementManageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            var deJure = false;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
            if (title != null && title.deJure == Hero.MainHero &&
                Settlement.CurrentSettlement.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                deJure = true;
            }

            return Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan ||
                   (deJure && Settlement.CurrentSettlement.IsVillage);
        }

        private static bool MenuVillageBuildingCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            var deJure = false;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
            if (title != null && title.deJure == Hero.MainHero &&
                Settlement.CurrentSettlement.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                deJure = true;
            }

            return (deJure || Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan) &&
                   Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuGuildManageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            var currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan &&
                   BannerKingsConfig.Instance.PopulationManager != null &&
                   BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement)
                   && BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement).EconomicData.Guild != null;
        }

        // -------- CONSEQUENCES ----------

        private static void MenuActionMeetNobilityConsequence(MenuCallbackArgs args)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionHuntingConsequence(MenuCallbackArgs args)
        {
            var meat = (int) (actionHuntGame * MBRandom.RandomFloatRanged(1f, 3f));
            var fur = (int) (actionHuntGame * MBRandom.RandomFloatRanged(0.5f, 2f));
            actionHuntGame = 0;

            MobileParty.MainParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("meat"), meat);
            MobileParty.MainParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("fur"), fur);
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionConsequenceWithGold(MenuCallbackArgs args)
        {
            GiveGoldAction.ApplyForSettlementToCharacter(Settlement.CurrentSettlement, Hero.MainHero, (int) actionGold);
            actionGold = 0f;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionConsequenceNeutral(MenuCallbackArgs args)
        {
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuCourtConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("court");
        }

        private static void MenuSettlementManageConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("population");
        }

        private static void MenuFaithConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("religions");
        }

        private static void MenuGuildManageConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("guild");
        }

        private static void MenuVillageProjectsConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("vilage_project");
        }

        private static void MenuTitlesConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("titles");
        }

        // -------- MENUS ----------

        private void SwitchToMenuIfThereIsAnInterrupt(string currentMenuId)
        {
            var genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
            if (genericStateMenu != currentMenuId)
            {
                if (!string.IsNullOrEmpty(genericStateMenu))
                {
                    GameMenu.SwitchToMenu(genericStateMenu);
                    return;
                }

                GameMenu.ExitToLast();
            }
        }

        private static void MenuWaitInit(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = true;
            args.MenuContext.GameMenu.StartWait();
            actionStart = CampaignTime.Now;
        }

        public static void MenuBannerKingsInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=!}Banner Kings");
        }
    }
}