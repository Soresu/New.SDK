using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.SDK.Core;


namespace SDKallista
{
    public static class HpBarDamageIndicator
    {
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;
        public static Color Color = Color.Lime;
        public static bool Enabled = true;
        private static DamageToUnitDelegate _damageToUnit;


        public static DamageToUnitDelegate DamageToUnit
        {
            get { return _damageToUnit; }

            set
            {
                if (_damageToUnit == null)
                {
                    Drawing.OnDraw += Drawing_OnDraw;
                }
                _damageToUnit = value;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled || _damageToUnit == null)
            {
                return;
            }

            foreach (var unit in
                 GameObjects.EnemyHeroes.Where(h => h.IsValid && h.IsHPBarRendered))
            {
                var barPos = unit.HPBarPosition;
                var damage = _damageToUnit(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var xPos = barPos.X + XOffset + Width * percentHealthAfterDamage;

                Drawing.DrawLine(xPos, barPos.Y + YOffset, xPos, barPos.Y + YOffset + Height, 2, Color);
            }
        }
    }
}
