using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class Graphic_CooldownWeapon : Graphic_Collection
    {
        public override Material MatSingle => subGraphics[0].MatSingle;

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_CooldownWeapon>(path, newShader, drawSize, newColor, newColorTwo, data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            if (thing == null)
            {
                return MatSingle;
            }
            return MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            if (thing == null)
            {
                return MatSingle;
            }
            return SubGraphicFor(thing).MatSingle;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            ((thing != null) ? SubGraphicFor(thing) : subGraphics[0]).DrawWorker(loc, rot, thingDef, thing, extraRotation);
            if (base.ShadowGraphic != null)
            {
                base.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            }
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            ((thing != null) ? SubGraphicFor(thing) : subGraphics[0]).Print(layer, thing, extraRotation);
            if (base.ShadowGraphic != null && thing != null)
            {
                base.ShadowGraphic.Print(layer, thing, extraRotation);
            }
        }

        private Graphic SubGraphicFor(Thing thing)
        {
            if (thing == null)
            {
                return subGraphics[0];
            }

            if (!thing.TryGetComp(out CompEquippable comp))
            {
                Log.ErrorOnce(thing.Label + ": Graphic_CooldownWeapon requires CompEquippable.", 4627811);
                return null;
            }
            
            if (comp.Holder == null || comp.Holder.stances == null || comp.Holder.stances.curStance == null)
            {
                return subGraphics[0];
            }

            for (int i = comp.VerbTracker.AllVerbs.Count - 1; i >= 0; i--)
            {
                Verb verb = comp.VerbTracker.AllVerbs[i];

                if (verb.Bursting)
                {
                    return subGraphics[3];
                }
            }

            if (comp.Holder.stances.curStance is Stance_Warmup)
            {
                return subGraphics[1];
            }

            if (comp.Holder.stances.curStance is Stance_Cooldown)
            {
                return subGraphics[2];
            }

            return subGraphics[0];
        }

        public override string ToString()
        {
            return "CooldownWeapon(path=" + path + ", count=" + subGraphics.Length + ")";
        }
    }
}
