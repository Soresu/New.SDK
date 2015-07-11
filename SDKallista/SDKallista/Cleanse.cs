using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Wrappers;

namespace SDKallista
{
    class Cleanse
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static int QssId = 3140;
        public static int MercurialId =3139;
        public static int DervishId =3137;

        public static bool QssUsed = false;
        public static void UseCleanse(Menu config)
        {
            if (QssUsed)
            {
                return;
            }
            if (Items.CanUseItem(QssId) && Items.HasItem(QssId))
            {
                CastItem(QssId, config);
            }
            if (Items.CanUseItem(MercurialId) && Items.HasItem(MercurialId))
            {
                CastItem(MercurialId, config);
            }
            if (Items.CanUseItem(DervishId) && Items.HasItem(DervishId))
            {
                CastItem(DervishId, config);
            }
        }

        private static void CastItem(int itemId, Menu config)
        {
            var delay = config["Misc"]["QSS"]["QSSdelay"].GetValue<MenuSlider>().Value;
            foreach (var buff in Player.Buffs)
            {
                if (config["Misc"]["QSS"]["slow"].GetValue<MenuBool>().Value && buff.Type == BuffType.Slow)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["blind"].GetValue<MenuBool>().Value && buff.Type == BuffType.Blind)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["silence"].GetValue<MenuBool>().Value && buff.Type == BuffType.Silence)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["snare"].GetValue<MenuBool>().Value && buff.Type == BuffType.Snare)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["stun"].GetValue<MenuBool>().Value && buff.Type == BuffType.Stun)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["charm"].GetValue<MenuBool>().Value && buff.Type == BuffType.Charm)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["taunt"].GetValue<MenuBool>().Value && buff.Type == BuffType.Taunt)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["fear"].GetValue<MenuBool>().Value && (buff.Type == BuffType.Fear || buff.Type == BuffType.Flee))
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["suppression"].GetValue<MenuBool>().Value && buff.Type == BuffType.Suppression)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["polymorph"].GetValue<MenuBool>().Value && buff.Type == BuffType.Polymorph)
                {
                    QssUsed = true;
                    DelayAction.Add(
                        delay, () =>
                        {
                            Items.UseItem(itemId, Player);
                            QssUsed = false;
                        });
                    return;
                }
                if (config["Misc"]["QSS"]["damager"].GetValue<MenuBool>().Value)
                {
                    switch (buff.Name)
                    {
                        case "zedulttargetmark":
                            QssUsed = true;
                            DelayAction.Add(
                                2900, () =>
                                {
                                    Items.UseItem(itemId, Player);
                                    QssUsed = false;
                                });
                            break;
                        case "VladimirHemoplague":
                            QssUsed = true;
                            DelayAction.Add(
                                4900, () =>
                                {
                                    Items.UseItem(itemId, Player);
                                    QssUsed = false;
                                });
                            break;
                        case "MordekaiserChildrenOfTheGrave":
                            QssUsed = true;
                            DelayAction.Add(
                                delay, () =>
                                {
                                    Items.UseItem(itemId, Player);
                                    QssUsed = false;
                                });
                            break;
                        case "urgotswap2":
                            QssUsed = true;
                            DelayAction.Add(
                                900, () =>
                                {
                                    Items.UseItem(itemId, Player);
                                    QssUsed = false;
                                });
                            break;
                        case "skarnerimpale":
                            QssUsed = true;
                            DelayAction.Add(
                                delay, () =>
                                {
                                    Items.UseItem(itemId, Player);
                                    QssUsed = false;
                                });
                            break;
                        case "poppydiplomaticimmunity":
                            QssUsed = true;
                            DelayAction.Add(
                                delay, () =>
                                {
                                    Items.UseItem(itemId, Player);
                                    QssUsed = false;
                                });
                            break;
                    }
                }
            }
        }
    }
}
