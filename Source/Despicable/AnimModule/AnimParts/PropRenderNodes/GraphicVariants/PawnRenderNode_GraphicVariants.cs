using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    public class PawnRenderNode_GraphicVariants : PawnRenderNode
    {
        protected new PawnRenderNodeProperties_GraphicVariants props;
        protected Graphic missingTextureGraphic;
        protected Dictionary<int, Graphic> variants;

        public Graphic getGraphicVariant(int variant)
        {
            if (variants == null || !variants.ContainsKey(variant))
            {
                return missingTextureGraphic;
            }

            return variants[variant];
        }

        public PawnRenderNode_GraphicVariants(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree) {
            this.props = (PawnRenderNodeProperties_GraphicVariants)props;
        }

        protected virtual Dictionary<int, Graphic> GraphicVariantsFor(Pawn pawn)
        {
            if (props.texPathVariantsDef == null)
            {
                return null;
            }

            return GenerateVariants(pawn, props.texPathVariantsDef);
        }

        protected override void EnsureMaterialVariantsInitialized(Graphic g)
        {
            if (variants == null)
            {
                variants = GraphicVariantsFor(this.tree.pawn);
            }
            if (missingTextureGraphic == null)
            {
                missingTextureGraphic = GenerateMissingTextureGraphic();
            }

            base.EnsureMaterialVariantsInitialized(g);
        }

        //used by all, including base classes, to create texPathVariants for pawn
        protected Dictionary<int, Graphic> GenerateVariants(Pawn pawn, TexPathVariantsDef texPathVariants)
        {

            Dictionary<int, Graphic> variantGraphics = new Dictionary<int, Graphic>();

            //for each graphic variant
            for (int i = 0; i < texPathVariants.variants.Count; i++)
            {

                //get new graphic
                Graphic variant = GraphicDatabase.Get<Graphic_Multi>(texPathVariants.variants[i], ShaderDatabase.CutoutSkinOverlay, Vector2.one, this.ColorFor(pawn));

                //add it to the variants dictionary; i + 1 for easier readability in logs
                variantGraphics.Add(i + 1, variant);

            }

            return variantGraphics;

        }

        protected Graphic GenerateMissingTextureGraphic()
        {
            return GraphicDatabase.Get<Graphic_Multi>("AnimationProps/MissingTexture/MissingTexture");
        }
    }
}
