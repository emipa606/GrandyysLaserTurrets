using RimWorld;
using Verse;

namespace DirectBeam;

public class DirectBeamProj : Projectile
{
    private MoteDualAttached LanceMote;

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var lanceMoteDef = GetComp<CompDirectBeam>().Props.lanceMoteDef;
        var centerVector = usedTarget.CenterVector3;
        base.Impact(hitThing, blockedByShield);
        var cell = centerVector.ToIntVec3();
        var v = centerVector - launcher.DrawPos;
        v.y = 0f;
        var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing,
            equipmentDef, def, targetCoverDef);
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        if (hitThing != null)
        {
            Pawn pawn;
            var notGuilty = (pawn = launcher as Pawn) == null || !pawn.Drafted;
            var damageInfo = new DamageInfo(def.projectile.damageDef, DamageAmount, ArmorPenetration,
                ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown,
                intendedTarget.Thing, notGuilty);
            hitThing.TakeDamage(damageInfo).AssociateWithLog(battleLogEntry_RangedImpact);
            var targetPawn = hitThing as Pawn;
            if (pawn?.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
            {
                pawn.stances.stagger.StaggerFor(95);
            }

            if (def.projectile.extraDamages != null)
            {
                foreach (var extraDamage in def.projectile.extraDamages)
                {
                    if (!Rand.Chance(extraDamage.chance))
                    {
                        continue;
                    }

                    var extraDamageInfo = new DamageInfo(extraDamage.def, extraDamage.amount,
                        extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null,
                        equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, notGuilty);
                    hitThing.TakeDamage(extraDamageInfo).AssociateWithLog(battleLogEntry_RangedImpact);
                }
            }

            if (Rand.Chance(def.projectile.bulletChanceToStartFire) &&
                (targetPawn == null || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(targetPawn))))
            {
                hitThing.TryAttachFire(def.projectile.bulletFireSizeRange.RandomInRange, launcher);
                return;
            }
        }
        else if (Rand.Chance(def.projectile.bulletChanceToStartFire))
        {
            FireUtility.TryStartFireIn(Position, map, def.projectile.bulletFireSizeRange.RandomInRange, launcher);
        }

        if (lanceMoteDef == null)
        {
            return;
        }

        LanceMote = MoteMaker.MakeInteractionOverlay(lanceMoteDef, launcher, new TargetInfo(cell, map));
        LanceMote.exactPosition = launcher.Position.ToVector3Shifted();
        LanceMote.exactRotation = v.ToAngleFlat();
    }
}