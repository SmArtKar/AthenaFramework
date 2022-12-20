using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using System;
using System.Linq;
using VFECore.Abilities;

namespace VFECore.Abilities
{
public class CompAbilityEffect_ConeShoot : CompAbilityEffect
{
	private List<IntVec3> tmpCells = new List<IntVec3>();

	private new CompProperties_AbilityConeShoot Props => (CompProperties_AbilityConeShoot)props;

	private Pawn Pawn => parent.pawn;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		IntVec3 position = parent.pawn.Position;
		float num = Mathf.Atan2(-(target.Cell.z - position.z), target.Cell.x - position.x) * 57.29578f;
		GenExplosion.DoExplosion(affectedAngle: new FloatRange(num - Props.angle/2f, num + Props.angle/2f), center: position, map: parent.pawn.MapHeld, radius: Props.range, damType: Props.damageDef, instigator: Pawn, damAmount: Props.damage, armorPenetration: Props.armorPenetration, explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: Props.spawnThingDef, postExplosionSpawnChance: Props.spawnThingChance, postExplosionSpawnThingCount: 1, postExplosionGasType: Props.spawnGasDef, applyDamageToExplosionCellsNeighbors: false, preExplosionSpawnThingDef: null, preExplosionSpawnChance: 0f, preExplosionSpawnThingCount: 1, chanceToStartFire: Props.chanceToStartFire, damageFalloff: false, direction: null, ignoredThings: null, doVisualEffects: false, propagationSpeed: Props.propagationSpeed, excludeRadius: Props.minRange, doSoundEffects: false);
		// Log.Message("range is " + Props.range);
		// List<IntVec3> AoECells = GenRadial.RadialCellsAround(position, Props.range, false).ToList();
		// Log.Message("AoECells has " + AoECells.Count);
		// if (Props.minRange > 0)
		// {
			// AoECells = AoECells.Except(GenRadial.RadialCellsAround(position, Props.minRange, false).ToList()).ToList();
		// }
		// Log.Message("AoECells has " + AoECells.Count);
		// float width = Props.angle;
		// List<IntVec3> ConeCells = new List<IntVec3>();
		// foreach (IntVec3 c in AoECells)
		// {
			// Log.Message("angle: " + Math.Abs(Mathf.Atan2(-(c.z - position.z), c.x - position.x) * 57.29578f) + " ; width : " + width);
			// if (Math.Abs(Mathf.Atan2(-(c.z - position.z), c.x - position.x) * 57.29578f) < width)
			// {
				// ConeCells.Add(c);
				// Log.Message("added a cell to ConeCells");
			// }
		// }
		// foreach (IntVec3 c in ConeCells)
		// {
			// Projectile newProjectile = (Projectile)GenSpawn.Spawn(WP_DefOf.WP_ProjectileTectonicShockwave, position, parent.pawn.MapHeld, WipeMode.Vanish);
            // newProjectile.Launch(parent.pawn, c, c, ProjectileHitFlags.IntendedTarget, false, null);
			// Log.Message("shot");
		// }
		base.Apply(target, dest);
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		//Log.Message("drawing field edges");
		GenDraw.DrawFieldEdges(AffectedCells(target));
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		if (Pawn.Faction != null)
		{
			foreach (IntVec3 item in AffectedCells(target))
			{
				List<Thing> thingList = item.GetThingList(Pawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].Faction == Pawn.Faction)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private List<IntVec3> AffectedCells(LocalTargetInfo target)
	{
		tmpCells.Clear();
		Vector3 vector = Pawn.Position.ToVector3Shifted().Yto0();
		IntVec3 intVec = target.Cell.ClampInsideMap(Pawn.Map);
		if (Pawn.Position == intVec)
		{
			return tmpCells;
		}
		float lengthHorizontal = (intVec - Pawn.Position).LengthHorizontal;
		float num = (float)(intVec.x - Pawn.Position.x) / lengthHorizontal;
		float num2 = (float)(intVec.z - Pawn.Position.z) / lengthHorizontal;
		intVec.x = Mathf.RoundToInt((float)Pawn.Position.x + num * Props.range);
		intVec.z = Mathf.RoundToInt((float)Pawn.Position.z + num2 * Props.range);
		float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
		float num3 = Props.lineWidthEnd / 2f;
		float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
		float num5 = 57.29578f * Mathf.Asin(num3 / num4);
		int num6 = GenRadial.NumCellsInRadius(Props.range);
		for (int i = 0; i < num6; i++)
		{
			IntVec3 intVec2 = Pawn.Position + GenRadial.RadialPattern[i];
			if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
			{
				tmpCells.Add(intVec2);
			}
		}
		List<IntVec3> list = GenSight.BresenhamCellsBetween(Pawn.Position, intVec);
		for (int j = 0; j < list.Count; j++)
		{
			IntVec3 intVec3 = list[j];
			if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
			{
				tmpCells.Add(intVec3);
			}
		}
		return tmpCells;
		bool CanUseCell(IntVec3 c)
		{
			if (!c.InBounds(Pawn.Map))
			{
				return false;
			}
			if (c == Pawn.Position)
			{
				return false;
			}
			if (c.Filled(Pawn.Map))
			{
				return false;
			}
			if (!c.InHorDistOf(Pawn.Position, Props.range))
			{
				return false;
			}
			return GenSight.LineOfSight(Pawn.Position, c, Pawn.Map, skipFirstCell: true);
		}
	}
}

public class CompProperties_AbilityConeShoot : CompProperties_AbilityEffect
{
	public float range;

	public float lineWidthEnd;

	public float angle;				//will be halved in calculations so it shoots 10° to one side and 10° to the other
	public int damage;			
	public DamageDef damageDef;		
	public float armorPenetration;	
	public float propagationSpeed;	//to not have instant "damage everything in this cone"
	public float minRange;			//to have cones separate from the shooter
	public ThingDef spawnThingDef;	//spawned post explosion
	public float spawnThingChance;
	public float chanceToStartFire;
	public GasType spawnGasDef;	

	public CompProperties_AbilityConeShoot()
	{
		compClass = typeof(CompAbilityEffect_ConeShoot);
	}
}
}