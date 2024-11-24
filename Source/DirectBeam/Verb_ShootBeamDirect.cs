using RimWorld;
using Verse;
using Verse.Sound;

namespace DirectBeam;

public class Verb_ShootBeamDirect : Verb_Shoot
{
    private Effecter endEffecter;
    private MoteDualAttached LanceMote;

    private Sustainer sustainer;

    public override float? AimAngleOverride
    {
        get
        {
            if (state != VerbState.Bursting)
            {
                return null;
            }

            return (currentTarget.CenterVector3 - caster.DrawPos).AngleFlat();
        }
    }

    public override void WarmupComplete()
    {
        base.WarmupComplete();
        var beamMoteDef = verbProps.beamMoteDef;
        var map = Caster.Map;
        var centerVector = currentTarget.CenterVector3;
        var cell = centerVector.ToIntVec3();
        if (beamMoteDef != null)
        {
            LanceMote = MoteMaker.MakeInteractionOverlay(beamMoteDef, caster, new TargetInfo(cell, map));
            LanceMote.Maintain();
        }

        if (verbProps.soundCastBeam != null)
        {
            sustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
        }
    }

    public override void BurstingTick()
    {
        base.BurstingTick();
        var vector = currentTarget.CenterVector3;
        var intVec = vector.ToIntVec3();
        var vector2 = vector - caster.Position.ToVector3Shifted();
        var num = vector2.MagnitudeHorizontal();
        var normalized = vector2.Yto0().normalized;
        var intVec2 =
            GenSight.LastPointOnLineOfSight(caster.Position, intVec, c => c.CanBeSeenOverFast(caster.Map), true);
        if (intVec2.IsValid)
        {
            num -= (intVec - intVec2).LengthHorizontal;
            vector = caster.Position.ToVector3Shifted() + (normalized * num);
            intVec = vector.ToIntVec3();
        }

        var offsetA = normalized * verbProps.beamStartOffset;
        var vector3 = vector - intVec.ToVector3Shifted();
        if (LanceMote != null)
        {
            LanceMote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(intVec, caster.Map),
                offsetA, vector3);
            LanceMote.Maintain();
        }

        if (endEffecter == null && verbProps.beamEndEffecterDef != null)
        {
            endEffecter = verbProps.beamEndEffecterDef.Spawn(intVec, caster.Map, vector3);
        }

        if (endEffecter != null)
        {
            endEffecter.offset = vector3;
            endEffecter.EffectTick(new TargetInfo(intVec, caster.Map), TargetInfo.Invalid);
            endEffecter.ticksLeft--;
        }

        if (verbProps.beamLineFleckDef != null)
        {
            var num2 = 1f * num;
            for (var i = 0; i < num2; i++)
            {
                if (!Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate(i / num2)))
                {
                    continue;
                }

                var vector4 = (i * normalized) - (normalized * Rand.Value) + (normalized / 2f);
                FleckMaker.Static(caster.Position.ToVector3Shifted() + vector4, caster.Map,
                    verbProps.beamLineFleckDef);
            }
        }

        sustainer?.Maintain();
    }
}