using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace Despicable
{
    [StaticConstructorOnStartup]
    public static class UIUtil
    {
        public static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");
        public static readonly Color LowLightBgColor = new Color(0.8f, 0.8f, 0.7f, 0.5f);
        public static readonly Color LowLightIconColor = new Color(0.8f, 0.8f, 0.7f, 0.6f);

        public static void DrawPawnImage(Pawn pawn, Rect inRect, bool shouldUpdate = false, bool renderHeadgear = true)
        {
            if (shouldUpdate)
                pawn.Drawer.renderer.SetAllGraphicsDirty();

            RenderTexture image = GetRenderTexture(pawn, fromCache: true, shouldUpdate, renderHeadgear);
            GUI.DrawTexture(inRect, image, ScaleMode.ScaleToFit);
        }

        private static RenderTexture GetRenderTexture(Pawn pawn, bool fromCache, bool shouldUpdate, bool renderHeadgear)
        {
            if (pawn == null)
                return null;
            if (fromCache)
                return PortraitsCache.Get(pawn, new Vector2(200f, 280f), Rot4.South);

            RenderTexture image = new RenderTexture(500, 700, 32, RenderTextureFormat.ARGB32);
            if (shouldUpdate)
            {
                PortraitsCache.SetDirty(pawn);
                pawn.Drawer.renderer.EnsureGraphicsInitialized();
                Render(pawn, image, renderHeadgear);
            }

            return image;
        }

        private static void Render(Pawn pawn, RenderTexture image, bool renderHeadgear)
        {
            float angle = 0f;
            Vector3 positionOffset = default(Vector3);
            if (pawn.Dead || pawn.Downed)
            {
                angle = 85f;
                positionOffset.x -= 0.18f;
                positionOffset.z -= 0.18f;
            }
            try
            {
                PawnCacheCameraManager.PawnCacheRenderer.RenderPawn(pawn, image, Vector3.zero, 1f, angle, Rot4.South, renderHead: true, renderHeadgear: renderHeadgear, renderClothes: true, portrait: true, positionOffset);
            }
            catch (Exception ex)
            {
                Log.Error(ex.StackTrace);
            }
        }

        public static void DrawSlot(Rect butRect, Texture icon = null, bool lowLight = true, string tooltipKey = null)
        {
            Material material = null;

            if (lowLight)
            {
                material = TexUI.GrayscaleGUI;
            }

            GUI.color = LowLightBgColor;
            GenUI.DrawTextureWithMaterial(butRect, BGTex, material);
            GUI.color = Color.white;

            if (icon != null)
            {
                DrawIcon(butRect, icon, material, lowLight);
            }

            Rect tooltipRect = new Rect(butRect);
            if (tooltipKey != null)
                TooltipHandler.TipRegionByKey(butRect, tooltipKey);
        }

        public static void DrawIcon(Rect rect, Texture icon, Material material = null, bool lowLight = false)
        {
            Texture badTex = icon;

            if (badTex == null)
            {
                badTex = BaseContent.BadTex;
            }
            if (!lowLight)
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.white.SaturationChanged(0f);
            }
            if (lowLight)
            {
                GUI.color = GUI.color.ToTransparent(0.6f);
            }
            Widgets.DrawTextureFitted(rect, badTex, 1f, material);
            GUI.color = Color.white;
        }

        public static string ColorTextHex(string text, string hex)
        {
            return ($"<color=#{hex}>" + text + "</color>");
        }
    }
}
