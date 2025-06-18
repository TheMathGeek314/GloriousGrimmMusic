using Modding;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using Satchel;
using Satchel.BetterMenus;

namespace GloriousGrimmMusic {
    public class GloriousGrimmMusic: Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings> {
        new public string GetName() => "GloriousGrimmMusic";
        public override string GetVersion() => "1.0.0.0";

        private Menu MenuRef;
        public static GlobalSettings gs { get; set; } = new();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            On.PlayMakerFSM.OnEnable += editFsm;
        }

        private void editFsm(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            if(gs.enabled) {
                if(self.gameObject.name == "Grimm Boss" && self.FsmName == "Control") {
                    self.GetValidState("Init").AddAction(new WhyWouldTeamCherryDoThis() { snapshot = self.GetValidState("Bow").GetFirstActionOfType<TransitionToAudioSnapshot>().snapshot, transitionTime = 0 });
                    var actionSnapshot = self.GetValidState("Music").GetFirstActionOfType<TransitionToAudioSnapshot>().snapshot.Value;
                    foreach(var state in self.FsmStates) {
                        if(state.GetFirstActionOfType<TransitionToAudioSnapshot>() != null) {
                            foreach(var action in state.Actions) {
                                if(action is TransitionToAudioSnapshot audio) {
                                    switch(audio.snapshot.Value.name) {
                                        case "Normal":
                                            audio.snapshot.Value = actionSnapshot;
                                            break;
                                        case "Normal Softer":
                                            action.Enabled = !gs.stagger;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates) {
            MenuRef ??= new Menu(
                name: "Glorious Grimm Music",
                elements: new Element[] {
                    new HorizontalOption(
                        name: "Enabled",
                        description: "",
                        values: new string[] { "On", "Off" },
                        applySetting: index => { gs.enabled = index == 0; },
                        loadSetting: () => gs.enabled ? 0 : 1
                    ),
                    new HorizontalOption(
                        name: "Stagger",
                        description: "Override softer version during staggers",
                        values: new string[] { "On", "Off" },
                        applySetting: index => { gs.stagger = index == 0; },
                        loadSetting: () => gs.stagger ? 0 : 1
                    )
                }
            );
            return MenuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu {
            get;
        }

        public void OnLoadGlobal(GlobalSettings s) {
            gs = s;
        }

        public GlobalSettings OnSaveGlobal() {
            return gs;
        }
    }

    public class WhyWouldTeamCherryDoThis: TransitionToAudioSnapshot {
        public override void OnEnter() {
            if(GameManager.instance.sceneName == "GG_Grimm" && GloriousGrimmMusic.gs.enabled) {
                base.OnEnter();
            }
            Finish();
        }
    }

    public class GlobalSettings {
        public bool enabled = true;
        public bool stagger = false;
    }
}